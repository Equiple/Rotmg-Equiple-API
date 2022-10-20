using Microsoft.Extensions.Options;
using RomgleWebApi.Authentication.AuthenticationValidators;
using RomgleWebApi.Data;
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
            Identity identity = new Identity
            {
                Provider = IdentityProvider.Self,
                Details = new IdentityDetails
                {
                    Name = "Itani"
                }
            };
            bool idExists;
            do
            {
                identity.Id = $"guest_{Guid.NewGuid()}";
                PlayerByIdentity? existingPlayer = await _playersService.GetByIdentityAsync(identity);
                idExists = existingPlayer.HasValue;
            }
            while (idExists);
            NewPlayer newPlayer = await _playersService.CreateNewAsync(identity);

            return await Success(newPlayer.Player, newPlayer.Device.Id);
        }

        public async Task<AuthenticationResult> AuthenticateAsync(
            AuthenticationPermit permit,
            string? playerId,
            string? deviceId)
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
            string actualDeviceId;
            if (string.IsNullOrWhiteSpace(playerId) || string.IsNullOrWhiteSpace(deviceId))
            {
                (player, Device device) = await AuthenticateAsync(identity, permit.DeviceId);
                actualDeviceId = device.Id;
            }
            else
            {
                player = await AddIdentity(playerId, identity, deviceId);
                actualDeviceId = deviceId;
            }

            return await Success(player, actualDeviceId);
        }

        public async Task<AuthenticationResult> RefreshAccessTokenAsync(
            string playerId,
            string deviceId,
            string refreshToken)
        {
            Player player = await _playersService.GetAsync(playerId);
            Device device = player.GetDevice(deviceId);
            RefreshToken? token = device.RefreshTokens.FirstOrDefault(token => token.Token == refreshToken);
            if (token == null)
            {
                return AuthenticationResult.Failure;
            }
            if (token.IsRevoked())
            {
                await LogoutAsync(player, deviceId);
            }
            if (!token.IsActive())
            {
                return AuthenticationResult.Failure;
            }
            token.Revoke();
            await _playersService.UpdateAsync(player);

            return await Success(player, deviceId);
        }

        public async Task LogoutAsync(string playerId, string deviceId)
        {
            Player player = await _playersService.GetAsync(playerId);
            await LogoutAsync(player, deviceId);
        }

        private async Task LogoutAsync(Player player, string deviceId)
        {
            Device device = player.GetDevice(deviceId);
            device.RevokeRefreshTokens();
            await _playersService.UpdateAsync(player);
            await _playersService.RefreshPersonalKeyAsync(player.Id, deviceId);
        }

        private async Task<(Player, Device)> AuthenticateAsync(Identity identity, string? deviceId)
        {
            PlayerByIdentity? playerByIdentity = await _playersService.GetByIdentityAsync(identity);
            Player player;
            Device device;
            if (playerByIdentity.HasValue)
            {
                player = playerByIdentity.Value.Player;
                if (string.IsNullOrWhiteSpace(deviceId))
                {
                    device = await _playersService.CreateNewDeviceAsync(player.Id);
                }
                else
                {
                    device = player.GetDevice(deviceId);
                }
                await _playersService.RefreshPersonalKeyAsync(player.Id, device.Id);
            }
            else
            {
                NewPlayer newPlayer = await _playersService.CreateNewAsync(identity);
                player = newPlayer.Player;
                device = newPlayer.Device;
            }

            return (player, device);
        }

        private async Task<Player> AddIdentity(string playerId, Identity identity, string deviceId)
        {
            Player player = await _playersService.GetAsync(playerId);
            player.Identities.Add(identity);
            await _playersService.UpdateAsync(player);
            await _playersService.RefreshPersonalKeyAsync(player.Id, deviceId);

            return player;
        }

        private async Task<AuthenticationResult> Success(Player player, string deviceId)
        {
            string accessToken = await _accessTokenService.GenerateAccessTokenAsync(player.Id, deviceId);
            RefreshToken refreshToken = await _accessTokenService.GenerateRefreshTokenAsync();
            Device device = player.GetDevice(deviceId);
            device.RefreshTokens.Add(refreshToken);
            await _playersService.UpdateAsync(player);

            return AuthenticationResult.Success(accessToken, refreshToken.Token, deviceId);
        }
    }
}
