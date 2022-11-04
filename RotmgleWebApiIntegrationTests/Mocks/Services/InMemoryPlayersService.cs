using Microsoft.Extensions.Options;
using RomgleWebApi.Data;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Models.Auth;
using RomgleWebApi.Data.Settings;
using RomgleWebApi.Extensions;
using RomgleWebApi.Services;
using RomgleWebApi.Utils;

namespace RotmgleWebApiTests.Mocks.Services
{
    internal class InMemoryPlayersService : IPlayersServiceMock, IPlayerService
    {
        private readonly List<Player> _players = new List<Player>();

        public IReadOnlyList<Player> Players => _players;

        public void SetInitialPlayers(params Player[] initialPlayers)
        {
            SetInitialPlayers((IEnumerable<Player>)initialPlayers);
        }

        public void SetInitialPlayers(IEnumerable<Player> initialPlayers)
        {
            _players.Clear();
            _players.AddRange(initialPlayers);
        }

        public async Task<Player> GetAsync(string id)
        {
            return _players.First(player => player.Id == id);
        }

        public async Task<PlayerByIdentity?> GetByIdentityAsync(Identity identity)
        {
            Player? player = _players
                .FirstOrDefault(player => player.Identities
                    .Any(playerIdentity => playerIdentity.Provider == identity.Provider
                        && playerIdentity.Id == identity.Id));
            if (player == null)
            {
                return null;
            }

            return new PlayerByIdentity(
                player,
                player.Identities.First(playerIdentity => playerIdentity.Provider == identity.Provider
                    && playerIdentity.Id == identity.Id));
        }

        public async Task<NewPlayer> CreateNewAsync(Identity identity)
        {
            PlayerByIdentity? existingPlayer = await GetByIdentityAsync(identity);
            if (existingPlayer.HasValue)
            {
                throw new Exception($"Player with given identity {identity.Provider}:{identity.Id} already exists");
            }

            NewPlayer newPlayer = PlayerUtils.Create(
                identity,
                DeviceUtils.GenerateDeviceId(),
                SecurityUtils.GenerateBase64SecurityKey());
            newPlayer.Player.Id = Guid.NewGuid().ToString();
            _players.Add(newPlayer.Player);

            return newPlayer;
        }

        public async Task<Device> CreateNewDeviceAsync(string playerId)
        {
            Player player = await GetAsync(playerId);
            Device newDevice = DeviceUtils.Create(
                DeviceUtils.GenerateDeviceId(),
                SecurityUtils.GenerateBase64SecurityKey());
            player.Devices.Add(newDevice);
            await UpdateAsync(player);

            return newDevice;
        }

        public async Task UpdateAsync(Player updatedPlayer)
        {
            int index = _players.FindIndex(player => player.Id == updatedPlayer.Id);
            _players[index] = updatedPlayer;
        }

        public async Task RefreshPersonalKeyAsync(string playerId, string deviceId)
        {
            Player player = await GetAsync(playerId);
            Device device = player.GetDevice(deviceId);
            device.PersonalKey = SecurityUtils.GenerateBase64SecurityKey();
            await UpdateAsync(player);
        }

        public Task<bool> WasDailyAttemptedAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetBestStreakAsync(string playerId, Gamemode mode)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetCurrentStreakAsync(string playerId, Gamemode mode)
        {
            throw new NotImplementedException();
        }

        public Task<DetailedGameStatistic> GetPlayerStatsAsync(string playerId, Gamemode mode)
        {
            throw new NotImplementedException();
        }

        public Task<PlayerProfile> GetPlayerProfileAsync(string playerId)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<PlayerProfile>> GetDailyLeaderboardAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<PlayerProfile>> GetNormalLeaderboardAsync()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetPlayerLeaderboardPlacementAsync(string playerId, Gamemode mode)
        {
            throw new NotImplementedException();
        }

        public Task InvalidateExpiredDailyGamesAsync()
        {
            throw new NotImplementedException();
        }

        public Task RemoveInactiveGuestAccountsAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdatePlayerScoreAsync(Player player, GameResult result)
        {
            throw new NotImplementedException();
        }
    }
}
