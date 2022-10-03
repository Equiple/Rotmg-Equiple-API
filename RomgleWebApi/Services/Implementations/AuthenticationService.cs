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

        public async Task<IAuthenticationResult> AuthenticateGuest(string? name)
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

        public Task<IAuthenticationResult> Authenticate(AuthenticationPermit permit) => Validated(permit, async identity =>
        {
            Player? player = await _playersService.GetByIdentityAsync(identity);
            player ??= await _playersService.CreateNewPlayerAsync(identity);

            return await NewTokens(player);
        });

        public Task<IAuthenticationResult> AddIdentity(string playerId, AuthenticationPermit permit) =>
            Validated(permit, async identity =>
        {
            Player player = await _playersService.GetAsync(playerId);
            player.Identities.Add(identity); //think about multiple identities with same provider
            await _playersService.UpdateAsync(player);

            return Success();
        });
        

        public async Task<IAuthenticationResult> RefreshAccessToken(string playerId, string refreshToken)
        {
            Player player = await _playersService.GetAsync(playerId);
            RefreshToken? token = player.RefreshTokens.FirstOrDefault(token => token.Token == refreshToken);
            if (token == null)
            {
                return Failure();
            }
            if (token.IsRevoked())
            {
                player.RevokeRefreshTokens();
            }
            if (!token.IsActive())
            {
                return Failure();
            }
            token.Revoke();

            return await NewTokens(player);
        }

        private async Task<IAuthenticationResult> Validated(
            AuthenticationPermit permit,
            Func<Identity, Task<IAuthenticationResult>> handler)
        {
            IAuthenticationValidator? validator = _settings.AuthenticationValidators
                .FirstOrDefault(validator => validator.IdentityProvider == permit.Provider);
            if (validator == null)
            {
                return Failure();
            }

            AuthenticationValidatorResult validationResult = await validator.Validate(permit);
            if (!validationResult.IsValid)
            {
                return Failure();
            }

            IAuthenticationResult result = await handler(validationResult.Identity!);
            return result;
        }

        private async Task<Result> NewTokens(Player player)
        {
            string accessToken = _accessTokenService.GenerateAccessToken(player.Id);
            RefreshToken refreshToken = await _accessTokenService.GenerateRefreshToken();
            player.RefreshTokens.Add(refreshToken);
            await _playersService.UpdateAsync(player);

            Result result = Success();
            result.AccessToken = accessToken;
            result.RefreshToken = refreshToken.Token;

            return result;
        }

        private static Result Success()
        {
            return new Result
            {
                IsAuthenticated = true
            };
        }

        private static Result Failure()
        {
            return new Result
            {
                IsAuthenticated = false
            };
        }

        private struct Result : IAuthenticationResult
        {
            public bool IsAuthenticated { get; set; }

            public string? AccessToken { get; set; }

            public string? RefreshToken { get; set; }
        }
    }
}
