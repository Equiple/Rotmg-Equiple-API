using Microsoft.Extensions.Options;
using RomgleWebApi.Authentication.AuthenticationValidators;
using RomgleWebApi.Data.Auth;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Models.Auth;
using RomgleWebApi.Data.Settings;
using RomgleWebApi.Extensions;

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

        public async Task<AuthenticationResult> AuthenticateGuestAsync()
        {
            const string guestIdentityId = "guest";
            Identity identity = new Identity
            {
                Provider = IdentityProvider.Self,
                Id = guestIdentityId,
                Details = new IdentityDetails
                {
                    Name = "Itani"
                }
            };
            Player player = await _playersService.CreateNewAsync(identity);

            return await Success(player);
        }

        public async Task<AuthenticationResult> AuthenticateAsync(
            AuthenticationPermit permit,
            string? playerId = null)
        {
            IAuthenticationValidator? validator = _settings.AuthenticationValidators
                .FirstOrDefault(validator => validator.IdentityProvider == permit.Provider);
            if (validator == null)
            {
                return AuthenticationResult.Failure;
            }
            AuthenticationValidatorResult validationResult = await validator.ValidateAsync(permit);
            if (!validationResult.IsValid)
            {
                return AuthenticationResult.Failure;
            }
            Identity identity = validationResult.Identity!;

            Player player;
            if (string.IsNullOrWhiteSpace(playerId))
            {
                player = await AuthenticateAsync(identity);
            }
            else
            {
                player = await AddIdentity(playerId, identity);
            }

            return await Success(player);
        }

        public async Task<AuthenticationResult> RefreshAccessTokenAsync(string playerId, string refreshToken)
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
                await _playersService.UpdateAsync(player);
                await _playersService.RefreshSecretKeyAsync(player.Id);
            }
            if (!token.IsActive())
            {
                return AuthenticationResult.Failure;
            }
            token.Revoke();
            await _playersService.UpdateAsync(player);

            return await Success(player);
        }

        public async Task LogoutAsync(string playerId)
        {
            await _playersService.RefreshSecretKeyAsync(playerId);
        }

        private async Task<Player> AuthenticateAsync(Identity identity)
        {
            Player? player = await _playersService.GetByIdentityAsync(identity);
            if (player == null)
            {
                player = await _playersService.CreateNewAsync(identity);
            }
            else
            {
                await _playersService.RefreshSecretKeyAsync(player.Id);
            }
            return player;
        }

        private async Task<Player> AddIdentity(string playerId, Identity identity)
        {
            Player player = await _playersService.GetAsync(playerId);
            player.Identities.Add(identity);
            await _playersService.UpdateAsync(player);
            await _playersService.RefreshSecretKeyAsync(player.Id);
            return player;
        }

        private async Task<AuthenticationResult> Success(Player player)
        {
            string accessToken = await _accessTokenService.GenerateAccessTokenAsync(player.Id);
            RefreshToken refreshToken = await _accessTokenService.GenerateRefreshTokenAsync();
            player.RefreshTokens.Add(refreshToken);
            await _playersService.UpdateAsync(player);

            return AuthenticationResult.Success(accessToken, refreshToken.Token);
        }
    }
}
