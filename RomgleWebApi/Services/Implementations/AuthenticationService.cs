using Microsoft.Extensions.Options;
using RomgleWebApi.Data.Auth;
using RomgleWebApi.Data.Extensions;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Models.Auth;
using RomgleWebApi.Data.Settings;
using RomgleWebApi.IdentityValidators;

namespace RomgleWebApi.Services.Implementations
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IPlayersService _playersService;
        private readonly IAccessTokenService _accessTokenService;
        private readonly AuthenticationServiceSettings _settings;

        public AuthenticationService(
            IPlayersService playersService,
            IAccessTokenService accessTokenService,
            IOptions<AuthenticationServiceSettings> settings)
        {
            _playersService = playersService;
            _accessTokenService = accessTokenService;
            _settings = settings.Value;
        }

        public async Task<AuthenticationResult> AuthenticateGuest(string? name)
        {
            const string guestIdentityId = "guest";
            Identity identity = new Identity
            {
                Provider = IdentityProvider.Self,
                Id = guestIdentityId,
                Details = new IdentityDetails
                {
                    Name = name
                }
            };
            Player player = await _playersService.CreateNewPlayerAsync(identity);

            return await NewTokens(player);
        }

        public Task<AuthenticationResult> Authenticate(AuthenticationPermit permit) => Validated(permit, async identity =>
        {
            Player? player = await _playersService.GetByIdentityAsync(identity);
            player ??= await _playersService.CreateNewPlayerAsync(identity);

            return await NewTokens(player);
        });

        public Task<AuthenticationResult> AddIdentity(string playerId, AuthenticationPermit permit) =>
            Validated(permit, async identity =>
        {
            Player player = await _playersService.GetAsync(playerId);
            player.Identities.Add(identity); //think about multiple identities with same provider
            await _playersService.UpdateAsync(player);

            return AuthenticationResult.Success;
        });
        

        public async Task<AuthenticationResult> RefreshAccessToken(string playerId, string refreshToken)
        {
            Player player = await _playersService.GetAsync(playerId);
            RefreshToken? token = player.RefreshTokens.FirstOrDefault(token => token.Token == refreshToken);
            if (token == null)
            {
                return AuthenticationResult.Failure;
            }
            if (token.IsRevoked())
            {
                player.RevokeRefreshTokens();
            }
            if (!token.IsActive())
            {
                return AuthenticationResult.Failure;
            }
            token.Revoke();

            return await NewTokens(player);
        }

        private async Task<AuthenticationResult> Validated(
            AuthenticationPermit permit,
            Func<Identity, Task<AuthenticationResult>> handler)
        {
            IAuthenticationValidator? validator = _settings.AuthenticationValidators
                .FirstOrDefault(validator => validator.IdentityProvider == permit.Provider);
            if (validator == null)
            {
                return AuthenticationResult.Failure;
            }

            AuthenticationValidatorResult validationResult = await validator.Validate(permit);
            if (!validationResult.IsValid)
            {
                return AuthenticationResult.Failure;
            }

            AuthenticationResult result = await handler(validationResult.Identity!);
            return result;
        }

        private async Task<AuthenticationResult> NewTokens(Player player)
        {
            string accessToken = _accessTokenService.GenerateAccessToken(player.Id);
            RefreshToken refreshToken = await _accessTokenService.GenerateRefreshToken();
            player.RefreshTokens.Add(refreshToken);
            await _playersService.UpdateAsync(player);

            return AuthenticationResult.NewTokens(accessToken, refreshToken.Token);
        }
    }
}
