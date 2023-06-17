using RotmgleWebApi.AuthenticationImplementation;
using RotmgleWebApi.Games;
using RotmgleWebApi.Players;

namespace RotmgleWebApiTests.Mocks
{
    internal class InMemoryPlayerService : IPlayerService, IPlayerServiceMock
    {
        private readonly List<Player> _players = new();

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

        public Task<Player> GetAsync(string id)
        {
            Player player = _players.First(p => p.Id == id);
            return Task.FromResult(player);
        }

        public Task<Player?> GetOrDefaultAsync(string id)
        {
            Player? player = _players.FirstOrDefault(p => p.Id == id);
            return Task.FromResult(player);
        }

        public Task<Player?> GetByIdentityAsync(Identity identity)
        {
            Player? player = _players.FirstOrDefault(p => p.Identities
                .Any(i => i.Provider == identity.Provider
                    && i.Id == identity.Id));
            return Task.FromResult(player);
        }

        public Task<Player> CreateNewAsync(string? name, Identity? identity)
        {
            Player player = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = name ?? "TestName",
                Role = "user",
                RegistrationDate = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow,
                Identities = new List<Identity>(),
                NormalStats = new GameStatistic(),
                DailyStats = new GameStatistic(),
                EndedGames = new List<Game>()
            };
            if (identity != null)
            {
                player.Identities.Add(identity);
            }
            _players.Add(player);

            return Task.FromResult(player);
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

        public Task<GameStatisticDetailed> GetPlayerStatsAsync(string playerId, Gamemode mode)
        {
            throw new NotImplementedException();
        }

        public Task<PlayerProfile> GetPlayerProfileAsync(string playerId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PlayerProfile>> GetDailyLeaderboardAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PlayerProfile>> GetNormalLeaderboardAsync()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetPlayerLeaderboardPlacementAsync(string playerId, Gamemode mode)
        {
            throw new NotImplementedException();
        }

        public Task UpdatePlayerScoreAsync(Player player, GameResult result)
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
    }
}
