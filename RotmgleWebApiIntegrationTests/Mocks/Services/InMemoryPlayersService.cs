using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Models.Auth;
using RomgleWebApi.Services;
using RomgleWebApi.Utils;

namespace RotmgleWebApiTests.Mocks.Services
{
    internal class InMemoryPlayersService : IPlayersServiceMock, IPlayersService
    {
        private readonly List<Player> _players = new List<Player>();

        public void SetInitialPlayers(params Player[] initialPlayers)
        {
            SetInitialPlayers((IEnumerable<Player>)initialPlayers);
        }

        public void SetInitialPlayers(IEnumerable<Player> initialPlayers)
        {
            _players.Clear();
            _players.AddRange(initialPlayers);
        }

        public Task<Player> CreateNewAsync(Identity identity)
        {
            Player? existingPlayer = GetByIdentityAsync(identity).Result;
            if (existingPlayer != null)
            {
                throw new Exception($"Player with given identity {identity.Provider}:{identity.Id} already exists");
            }

            Player newPlayer = PlayerUtils.Create(identity, SecurityUtils.GenerateBase64SecurityKey());
            newPlayer.Id = Guid.NewGuid().ToString();
            _players.Add(newPlayer);

            return Task.FromResult(newPlayer);
        }

        public async Task RefreshSecretKeyAsync(string playerId)
        {
            Player player = await GetAsync(playerId);
            player.SecretKey = SecurityUtils.GenerateBase64SecurityKey();
            await UpdateAsync(player);
        }

        public Task<Player> GetAsync(string id)
        {
            return Task.FromResult(_players.First(player => player.Id == id));
        }

        public Task<Player?> GetByIdentityAsync(Identity identity)
        {
            return Task.FromResult(_players
                .FirstOrDefault(player => player.Identities
                    .Any(playerIdentity =>
                        playerIdentity.Provider == identity.Provider &&
                        playerIdentity.Id == identity.Id)));
        }

        public Task<Player?> GetByRefreshTokenAsync(string refreshToken)
        {
            return Task.FromResult(_players
                .FirstOrDefault(player => player.RefreshTokens
                    .Any(token => token.Token == refreshToken)));
        }

        public Task UpdateAsync(Player updatedPlayer)
        {
            int index = _players.FindIndex(player => player.Id == updatedPlayer.Id);
            _players[index] = updatedPlayer;
            return Task.CompletedTask;
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
    }
}
