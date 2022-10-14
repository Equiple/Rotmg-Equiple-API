using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using RomgleWebApi.DAL;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Models.Auth;
using RomgleWebApi.Data.Settings;
using RomgleWebApi.Extensions;
using RomgleWebApi.Utils;

namespace RomgleWebApi.Services.Implementations
{
    public class PlayersService : IPlayersService
    {
        private readonly IMongoCollection<Player> _playersCollection;
        private readonly IItemsService _itemsService;

        public PlayersService(
            IOptions<RotmgleDatabaseSettings> rotmgleDatabaseSettings,
            IDataCollectionProvider dataCollectionProvider,
            IItemsService itemsService)
        {
            _playersCollection = dataCollectionProvider
                .GetDataCollection<Player>(rotmgleDatabaseSettings.Value.PlayersCollectionName)
                .AsMongo();
            _itemsService = itemsService;
        }

        public async Task<Player> GetAsync(string playerId)
        {
            Player player = await _playersCollection.Find(player => player.Id == playerId).FirstAsync();
            return player;
        }

        public async Task<Player?> GetByIdentityAsync(Identity identity)
        {
            Player? player = await _playersCollection
                .Find(player => player.Identities
                    .Any(playerIdentity =>
                        playerIdentity.Provider == identity.Provider &&
                        playerIdentity.Id == player.Id))
                .FirstOrDefaultAsync();
            return player;
        }

        public async Task<Player?> GetByRefreshTokenAsync(string refreshToken)
        {
            Player? player = await _playersCollection
                .Find(player => player.RefreshTokens
                    .Any(token => token.Token == refreshToken))
                .FirstOrDefaultAsync();
            return player;
        }

        public async Task<Player> CreateNewAsync(Identity identity)
        {
            Player? existingPlayer = await GetByIdentityAsync(identity);
            if (existingPlayer != null)
            {
                throw new Exception($"Player with given identity {identity.Provider}:{identity.Id} already exists");
            }

            string secretKey = await GenerateUniqueSecretKey();
            Player newPlayer = PlayerUtils.Create(identity, secretKey);
            await _playersCollection.InsertOneAsync(newPlayer);

            return newPlayer;
        }

        public async Task UpdateAsync(Player updatedPlayer)
        {
            await _playersCollection.ReplaceOneAsync(player => player.Id == updatedPlayer.Id, updatedPlayer);
        }

        public async Task RefreshSecretKeyAsync(string playerId)
        {
            Player player = await GetAsync(playerId);
            string key = await GenerateUniqueSecretKey();
            player.SecretKey = key;
            await UpdateAsync(player);
        }

        public async Task<bool> WasDailyAttemptedAsync(string id)
        {
            Player player = await GetAsync(id);
            if (player.EndedGames.Any(game =>
                game.Mode == Gamemode.Daily &&
                game.StartDate.Date == DateTime.UtcNow.Date))
            {
                return true;
            }
            return false;
        }

        public async Task<int> GetBestStreakAsync(string playerId, Gamemode mode)
        {
            Player player = await GetAsync(playerId);
            return player.GetStats(mode).BestStreak;
        }

        public async Task<int> GetCurrentStreakAsync(string playerId, Gamemode mode)
        {
            Player currentPlayer = await GetAsync(playerId);
            return currentPlayer.GetStats(mode).CurrentStreak;
        }

        public async Task<DetailedGameStatistic> GetPlayerStatsAsync(string playerId, Gamemode mode)
        {
            Player player = await GetAsync(playerId);
            DetailedGameStatistic stats = await player.GetStats(mode).ToDetailed(_itemsService);
            return stats;
        }

        public async Task<PlayerProfile> GetPlayerProfileAsync(string playerId)
        {
            Player player = await GetAsync(playerId);
            PlayerProfile playerProfile = await player.ToProfileAsync(_itemsService);
            return playerProfile;
        }

        public async Task<IReadOnlyList<PlayerProfile>> GetDailyLeaderboardAsync()
        {
            List<Player> players = await _playersCollection
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
                .OrderByDescending(item => item.dailyGuesses)
                .ThenByDescending(item => item.player.DailyStats.CurrentStreak);
            PlayerProfile[] profiles = await Task.WhenAll(intermediatePlayers
                .Take(10)
                .Select(item => item.player
                    .ToProfileAsync(_itemsService, dailyGuesses: item.dailyGuesses)));
            return profiles;
        }

        public async Task<IReadOnlyList<PlayerProfile>> GetNormalLeaderboardAsync()
        {
            //await CreateAdminAccount();
            //await CreateKnockOffPlayers(12);
            List<Player> leaderboard = await _playersCollection
                .AsQueryable()
                .Where(player => player.EndedGames
                    .Any(game => game.Mode == Gamemode.Normal
                        && game.IsEnded
                        && game.GameResult == GameResult.Won))
                .OrderByDescending(player => player.NormalStats.CurrentStreak)
                .ThenByDescending(player => player.NormalStats.RunsWon)
                .ToListAsync();
            PlayerProfile[] profiles = await Task.WhenAll(leaderboard
                .Take(10)
                .Select(player => player
                    .ToProfileAsync(_itemsService, dailyGuesses: player.EndedGames
                        .Find(game => game.Mode == Gamemode.Normal
                            && game.IsEnded
                            && game.GameResult == GameResult.Won)!.GuessItemIds.Count)));
            return profiles;
        }

        public async Task<int> GetPlayerLeaderboardPlacementAsync(string playerId, Gamemode mode)
        {
            Player player = await GetAsync(playerId);
            IReadOnlyList<PlayerProfile> leaderboard;
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

        #region private methods

        private async Task<string> GenerateUniqueSecretKey()
        {
            string key;
            bool alreadyExists;
            do
            {
                key = SecurityUtils.GenerateBase64SecurityKey();
                long existingCount = await _playersCollection.CountDocumentsAsync(player => player.SecretKey == key);
                alreadyExists = existingCount > 0;
            }
            while (alreadyExists);
            return key;
        }

        private async Task<IReadOnlyList<Player>> CreateKnockOffPlayers(int amount)
        {
            List<Player> players = new List<Player>();
            for (int i = 0; i < amount; i++)
            {
                Identity identity = new Identity
                {
                    Provider = IdentityProvider.Self,
                    Id = "knock_off",
                    Details = new IdentityDetails
                    {
                        Name = StringUtils.GenerateRandomNameLookingString()
                    }
                };
                Player player = await CreateNewAsync(identity);
                player.Randomize();
                await UpdateAsync(player);
                players.Add(player);
            }
            return players;
        }

        private async Task CreateAdminAccount()
        {
            IReadOnlyList<Player> player = await CreateKnockOffPlayers(1);
            player[0].Name = "admin";
            await UpdateAsync(player[0]);
        }

        #endregion private methods
    }
}
