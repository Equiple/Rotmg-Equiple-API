using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using RotmgleWebApi.AuthenticationImplementation;
using RotmgleWebApi.Games;
using RotmgleWebApi.Items;

namespace RotmgleWebApi.Players
{
    public class PlayerService : IPlayerService
    {
        private readonly IMongoCollection<Player> _playerCollection;
        private readonly IItemService _itemService;

        public PlayerService(
            IOptions<RotmgleDatabaseOptions> rotmgleDatabaseSettings,
            IItemService itemService)
        {
            _playerCollection = MongoUtils.GetCollection<Player>(
                rotmgleDatabaseSettings.Value,
                rotmgleDatabaseSettings.Value.PlayerCollectionName);
            _itemService = itemService;
        }

        #region public methods

        public Task<Player> GetAsync(string playerId)
        {
            return _playerCollection
                .Find(player => player.Id == playerId)
                .FirstAsync();
        }

        public Task<Player?> GetOrDefaultAsync(string playerId)
        {
            return _playerCollection
                .Find(player => player.Id == playerId)
                .FirstOrDefaultAsync();
        }

        public async Task InvalidateExpiredDailyGamesAsync()
        {
            IMongoQueryable<Player> players = _playerCollection
                .AsQueryable()
                .Where(player => player.CurrentGame != null
                    && !player.CurrentGame.IsEnded
                    && player.CurrentGame.Mode == Gamemode.Daily);
            foreach (Player player in players)
            {
                await UpdatePlayerScoreAsync(player, GameResult.Lost);
            }
        }

        public async Task RemoveInactiveGuestAccountsAsync()
        {
            DateTime weekAgo = DateTime.UtcNow.Date.AddDays(-7);
            await _playerCollection.DeleteManyAsync(player => player.Identities.Count == 0
                    && player.LastSeen.Date < weekAgo);
        }

        public async Task<Player?> GetByIdentityAsync(Identity identity)
        {
            Player? player = await _playerCollection
                .Find(player => player.Identities
                    .Where(playerIdentity => playerIdentity.Provider == identity.Provider
                        && playerIdentity.Id == identity.Id)
                    .Any())
                .FirstOrDefaultAsync();
            return player;
        }

        public async Task<Player> CreateNewAsync(string? name, Identity? identity)
        {
            if (identity != null)
            {
                Player? existingPlayer = await GetByIdentityAsync(identity);
                if (existingPlayer != null)
                {
                    throw new Exception($"Player with given identity {identity.Provider}:{identity.Id} already exists");
                }
            }

            name ??= StringUtils.GetRandomDefaultName();
            Player player = new()
            {
                Name = name,
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
            await _playerCollection.InsertOneAsync(player);

            return player;
        }

        public async Task UpdateAsync(Player player)
        {
            if (player.Id == null)
            {
                throw new Exception("Player id is null");
            }
            player.LastSeen = DateTime.UtcNow;
            ReplaceOneResult result = await _playerCollection
                .ReplaceOneAsync(p => p.Id == player.Id, player);
            if (!result.IsAcknowledged || result.ModifiedCount == 0)
            {
                throw new Exception("Player was not updated");
            }
        }

        public async Task<bool> WasDailyAttemptedAsync(string playerId)
        {
            Player player = await GetAsync(playerId);
            return player.EndedGames
                .Where(game => game.Mode == Gamemode.Daily
                    && game.StartDate.Date == DateTime.UtcNow.Date)
                .Any();
        }

        public async Task<int> GetBestStreakAsync(string playerId, Gamemode mode)
        {
            Player player = await GetAsync(playerId);
            int bestStreak = player.GetStats(mode).BestStreak;
            return bestStreak;
        }

        public async Task<int> GetCurrentStreakAsync(string playerId, Gamemode mode)
        {
            Player player = await GetAsync(playerId);
            int currentStreak = player.GetStats(mode).CurrentStreak;
            return currentStreak;
        }

        public async Task<GameStatisticDetailed> GetPlayerStatsAsync(string playerId, Gamemode mode)
        {
            Player player = await GetAsync(playerId);
            GameStatisticDetailed stats = await player.GetStats(mode).ToDetailed(_itemService);
            return stats;
        }

        public async Task<PlayerProfile> GetPlayerProfileAsync(string playerId)
        {
            Player player = await GetAsync(playerId);
            PlayerProfile playerProfile = await player.ToProfileAsync(_itemService);
            return playerProfile;
        }

        public async Task<IEnumerable<PlayerProfile>> GetDailyLeaderboardAsync()
        {
            List<Player> players = await _playerCollection
                .Find(player => player.EndedGames
                    .Where(game => game.Mode == Gamemode.Daily
                        && game.StartDate.Date == DateTime.UtcNow.Date
                        && game.IsEnded
                        && game.GameResult == GameResult.Won)
                    .Any())
                .ToListAsync();
            IEnumerable<(Player player, int dailyGuesses)> intermediatePlayers = players
                .Select(player => (player, dailyGuesses: player.EndedGames
                    .First(game => game.Mode == Gamemode.Daily
                        && game.StartDate.Date == DateTime.UtcNow.Date
                        && game.IsEnded
                        && game.GameResult == GameResult.Won)
                    .GuessItemIds.Count))
                .OrderBy(x => x.dailyGuesses)
                .ThenByDescending(x => x.player.DailyStats.CurrentStreak);
            PlayerProfile[] profiles = await Task.WhenAll(intermediatePlayers
                .Take(10)
                .Select(item => item.player
                    .ToProfileAsync(_itemService, dailyGuesses: item.dailyGuesses)));
            return profiles;
        }

        public async Task<IEnumerable<PlayerProfile>> GetNormalLeaderboardAsync()
        {
            //await CreateAdminAccount();
            //await CreateKnockOffPlayers(12);
            List<Player> leaderboard = await _playerCollection
                .AsQueryable()
                .Where(player => player.EndedGames
                    .Where(game => game.Mode == Gamemode.Normal
                        && game.IsEnded
                        && game.GameResult == GameResult.Won)
                    .Any())
                .OrderByDescending(player => player.NormalStats.CurrentStreak)
                .ThenByDescending(player => player.NormalStats.RunsWon)
                .ToListAsync();
            PlayerProfile[] profiles = await Task.WhenAll(leaderboard
                .Take(10)
                .Select(player => player
                    .ToProfileAsync(_itemService, dailyGuesses: player.EndedGames
                        .Find(game => game.Mode == Gamemode.Normal
                            && game.IsEnded
                            && game.GameResult == GameResult.Won)!.GuessItemIds.Count)));
            return profiles;
        }

        public async Task<int> GetPlayerLeaderboardPlacementAsync(string playerId, Gamemode mode)
        {
            IEnumerable<PlayerProfile> leaderboard;
            if (mode == Gamemode.Daily)
            {
                leaderboard = await GetDailyLeaderboardAsync();

            }
            else if (mode == Gamemode.Normal)
            {
                leaderboard = await GetNormalLeaderboardAsync();
            }
            else
            {
                throw new Exception($"Exception at {nameof(GetPlayerLeaderboardPlacementAsync)} method, {nameof(GameService)} class: " +
                    $"Invalid {nameof(Gamemode)} value: [{mode}]\n");
            }
            return leaderboard.FirstIndex(player => player.Id == playerId);
        }

        public async Task UpdatePlayerScoreAsync(Player player, GameResult result)
        {
            if (player.CurrentGame == null)
            {
                throw new Exception("Player has no CurrentGame");
            }
            if (player.CurrentGame.Mode == Gamemode.Normal)
            {
                if (result == GameResult.Won)
                {
                    player.NormalStats = player.NormalStats.AddWin(
                        player.CurrentGame.TargetItemId,
                        player.CurrentGame.GuessItemIds.Count);
                }
                else if (result == GameResult.Lost)
                {
                    player.NormalStats = player.NormalStats.AddLose();
                }
            }
            else if (player.CurrentGame.Mode == Gamemode.Daily)
            {
                if (result == GameResult.Won)
                {
                    player.DailyStats = player.DailyStats.AddWin(
                        player.CurrentGame.TargetItemId,
                        player.CurrentGame.GuessItemIds.Count);
                }
                else if (result == GameResult.Lost)
                {
                    player.DailyStats = player.DailyStats.AddLose();
                }
            }
            player.CurrentGame.IsEnded = true;
            player.CurrentGame.GameResult = result;
            player.EndedGames.Add(player.CurrentGame);
            await UpdateAsync(player);
        }

        #endregion

        #region private methods

        private async Task<IReadOnlyList<Player>> CreateKnockOffPlayers(int amount)
        {
            List<Player> players = new();
            for (int i = 0; i < amount; i++)
            {
                Result<Player> createRes = await CreateNewAsync(
                    StringUtils.GenerateRandomNameLookingString(),
                    null);
                if (createRes is Result<Player>.Ok playerRes)
                {
                    players.Add(playerRes.Value);
                }
            }
            return players;
        }

        private async Task CreateAdminAccount()
        {
            IReadOnlyList<Player> player = await CreateKnockOffPlayers(1);
            player[0].Name = "admin";
            player[0].Role = "admin";
            await UpdateAsync(player[0]);
        }

        #endregion
    }
}
