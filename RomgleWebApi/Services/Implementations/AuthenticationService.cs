using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NUnit.Framework.Internal;
using RomgleWebApi.Authentication.AuthenticationValidators;
using RomgleWebApi.DAL;
using RomgleWebApi.Data;
using RomgleWebApi.Data.Auth;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Models.Auth;
using RomgleWebApi.Data.Settings;
using RomgleWebApi.Extensions;
using RomgleWebApi.Utils;

namespace RomgleWebApi.Services.Implementations
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IPlayerService _playerService;
        private readonly IAccessTokenService _accessTokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly AuthenticationServiceSettings _settings;

        public AuthenticationService(
            IPlayerService playersService,
            IAccessTokenService accessTokenService,
            IRefreshTokenService refreshTokenService,
            IOptions<AuthenticationServiceSettings> settings)
        {
            _playerService = playersService;
            _accessTokenService = accessTokenService;
            _refreshTokenService = refreshTokenService;
            _settings = settings.Value;
        }

        public async Task<AuthenticationResult> AuthenticateGuestAsync()
        {
            Identity identity = new Identity
            {
                Provider = IdentityProvider.Self,
                Details = new IdentityDetails
                {
                    Name = StringUtils.GetRandomDefaultName()
                }
            };
            bool idExists;
            do
            {
                identity.Id = $"guest_{Guid.NewGuid()}";
                PlayerByIdentity? existingPlayer = await _playerService.GetByIdentityAsync(identity);
                idExists = existingPlayer.HasValue;
            }
            while (idExists);
            NewPlayer newPlayer = await _playerService.CreateNewAsync(identity);

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
            Player player = await _playerService.GetAsync(playerId);
            //Device device = player.GetDevice(deviceId);
            //RefreshToken? token = device.RefreshTokens.FirstOrDefault(token => token.Token == refreshToken);
            RefreshToken? token = await _refreshTokenService.GetTokenOrDefaultAsync(refreshToken);
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
            await _playerService.UpdateAsync(player);
            await _refreshTokenService.UpdateAsync(token);
            return await Success(player, deviceId);
        }

        public async Task LogoutAsync(string playerId, string deviceId)
        {
            Player player = await _playerService.GetAsync(playerId);
            await LogoutAsync(player, deviceId);
        }

        private async Task LogoutAsync(Player player, string deviceId)
        {
            Device device = player.GetDevice(deviceId);
            await _refreshTokenService.RevokeRefreshTokens(deviceId);
            await _playerService.RefreshPersonalKeyAsync(player.Id, deviceId);
        }

        private async Task<(Player, Device)> AuthenticateAsync(Identity identity, string? deviceId)
        {
            PlayerByIdentity? playerByIdentity = await _playerService.GetByIdentityAsync(identity);
            Player player;
            Device device;
            if (playerByIdentity.HasValue)
            {
                player = playerByIdentity.Value.Player;
                if (string.IsNullOrWhiteSpace(deviceId))
                {
                    device = await _playerService.CreateNewDeviceAsync(player.Id);
                }
                else
                {
                    device = player.GetDevice(deviceId);
                }
                await _playerService.RefreshPersonalKeyAsync(player.Id, device.Id);
            }
            else
            {
                NewPlayer newPlayer = await _playerService.CreateNewAsync(identity);
                player = newPlayer.Player;
                device = newPlayer.Device;
            }
            return (player, device);
        }

        private async Task<Player> AddIdentity(string playerId, Identity identity, string deviceId)
        {
            Player player = await _playerService.GetAsync(playerId);
            player.Identities.Add(identity);
            await _playerService.UpdateAsync(player);
            await _playerService.RefreshPersonalKeyAsync(player.Id, deviceId);
            return player;
        }

        private async Task<AuthenticationResult> Success(Player player, string deviceId)
        {
            string accessToken = await _accessTokenService.GenerateAccessTokenAsync(player.Id, deviceId);
            RefreshToken refreshToken = await _accessTokenService.GenerateRefreshTokenAsync(deviceId);
            return AuthenticationResult.Success(accessToken, refreshToken.Token, deviceId);
        }
    }
}
