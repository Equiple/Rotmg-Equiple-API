using MongoDB.Driver;
using RotmgleWebApi.Players;

namespace RotmgleWebApi.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IEnumerable<IAuthenticationValidator> _validators;
        private readonly IPlayerService _playerService;
        private readonly IAccessTokenService _accessTokenService;

        public AuthenticationService(
            IEnumerable<IAuthenticationValidator> validators,
            IAccessTokenService accessTokenService,
            IPlayerService playersService)
        {
            _validators = validators;
            _accessTokenService = accessTokenService;
            _playerService = playersService;
        }

        public async Task<AuthenticationResult> AuthenticateGuestAsync(string deviceId)
        {
            Identity identity = new()
            {
                Provider = IdentityProvider.Self,
                Id = $"guest_{Guid.NewGuid()}",
            };
            Player player = await _playerService.CreateNewAsync(identity, deviceId);
            AuthenticationResult result = await GenerateNewTokensAndUpdatePlayer(player, deviceId);
            return result;
        }

        public async Task<Result<AuthenticationResult>> AuthenticateAsync(
            string? loggedPlayerId,
            string deviceId,
            AuthenticationPermit permit)
        {
            IAuthenticationValidator? validator = _validators
                .FirstOrDefault(validator => validator.IdentityProvider == permit.Provider);
            if (validator == null)
            {
                return new Exception("Identity provider not supported");
            }

            Result<AuthenticationValidatorResult> validationResult = await validator.ValidateAsync(permit);
            AuthenticationValidatorResult validationRes;
            switch (validationResult)
            {
                case Result<AuthenticationValidatorResult>.Ok ok:
                    validationRes = ok.Value;
                    break;
                case Result<AuthenticationValidatorResult>.Error error:
                    return error.Exception;
                default:
                    throw new NotSupportedException();
            }

            Player? loggedPlayer = loggedPlayerId != null
                ? await _playerService.GetAsync(loggedPlayerId)
                : null;
            Player? player = await _playerService.GetByIdentityAsync(validationRes.Identity);

            AuthenticationResult result;

            if (player != null)
            {
                if (loggedPlayer != null && !loggedPlayer.IsGuest())
                {
                    if (loggedPlayer.Id == player.Id)
                    {
                        return new Exception("Already logged in");
                    }
                    return new Exception("Logout first");
                }
                result = await GenerateNewTokensAndUpdatePlayer(player, deviceId);
                return result;
            }

            if (loggedPlayer != null)
            {
                loggedPlayer.Identify(validationRes.Identity, validationRes.Name);
                result = await GenerateNewTokensAndUpdatePlayer(loggedPlayer, deviceId);
                return result;
            }

            player = await _playerService.CreateNewAsync(
                validationRes.Identity,
                deviceId,
                name: validationRes.Name);
            result = await GenerateNewTokensAndUpdatePlayer(player, deviceId);
            return result;
        }

        public async Task<Result<AuthenticationResult>> RefreshAccessTokenAsync(
            string playerId,
            string deviceId,
            string refreshToken)
        {
            Player player = await _playerService.GetAsync(playerId);
            Device? device = player.GetDevice(deviceId);

            if (device == null)
            {
                return new Exception("Device not found");
            }

            if (device.RefreshToken == null || device.RefreshToken.IsRevoked())
            {
                return new Exception("Logged out");
            }

            if (device.RefreshToken.IsExpired())
            {
                return new Exception("Refresh token expired");
            }

            if (device.RefreshToken.Token != refreshToken)
            {
                await LogoutAsync(player, deviceId);
                return new Exception("Invalid refresh token");
            }

            AuthenticationResult result = await GenerateNewTokensAndUpdatePlayer(player, deviceId);
            return result;
        }

        public async Task LogoutAsync(string playerId, string deviceId)
        {
            Player player = await _playerService.GetAsync(playerId);
            await LogoutAsync(player, deviceId);
        }

        private async Task LogoutAsync(Player player, string deviceId)
        {
            Device? device = player.GetDevice(deviceId);
            if (device?.RefreshToken != null)
            {
                device.RefreshToken.Revoke();
            }
            await _playerService.UpdateAsync(player);
        }

        private async Task<AuthenticationResult> GenerateNewTokensAndUpdatePlayer(Player player, string deviceId)
        {
            string accessToken = _accessTokenService.GenerateAccessToken(player, deviceId);
            RefreshToken refreshToken = _accessTokenService.GenerateRefreshToken();
            Device device = player.GetOrCreateDevice(deviceId);
            device.RefreshToken = refreshToken;
            await _playerService.UpdateAsync(player);
            return new AuthenticationResult(accessToken, refreshToken.Token);
        }
    }
}
