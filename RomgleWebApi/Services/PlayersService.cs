using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using RomgleWebApi.Data;
using RomgleWebApi.Data.Extensions;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Utils;
using StringExtensions = RomgleWebApi.Data.Extensions.StringExtensions;

namespace RomgleWebApi.Services
{
    public class PlayersService
    {
        private readonly IMongoCollection<Player> _playersCollection;
        private readonly ItemsService _itemsService;

        public PlayersService(
            IOptions<RotmgleDatabaseSettings> rotmgleDatabaseSettings, ItemsService itemService)
        {
            var mongoClient = new MongoClient(
                rotmgleDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                rotmgleDatabaseSettings.Value.DatabaseName);

            _playersCollection = mongoDatabase.GetCollection<Player>(
                rotmgleDatabaseSettings.Value.PlayersCollectionName);

            _itemsService = itemService;
        }

        #region public_methods

        public async Task<List<Player>> GetAsync() =>
            await _playersCollection.Find(_ => true).ToListAsync();

        public async Task<Player> GetAsync(string id)
        {
            return await _playersCollection.Find(x => x.Id == id).FirstAsync();
        }

        public async Task CreateAsync(Player newPlayer) =>
            await _playersCollection.InsertOneAsync(newPlayer);

        public async Task UpdateAsync(string id, Player updatedPlayer) =>
            await _playersCollection.ReplaceOneAsync(x => x.Id == id, updatedPlayer);

        public async Task RemoveAsync(string id) =>
            await _playersCollection.DeleteOneAsync(x => x.Id == id);

        public async Task<List<Player>> FindAllAsync(string searchInput) =>
            await _playersCollection.Find(x => x.Name.Contains(searchInput)).ToListAsync();

        public async Task<bool> DoesExistAsync(string playerId)=>
            await GetAsync(playerId) != null;

        public async Task<bool> DoesExistAsync(Player player)=>
            await _playersCollection.CountDocumentsAsync(x => x.Email == player.Email) == 1;

        public async Task<bool> WasDailyAttemptedAsync(string playerId)
        {
            Player currentPlayer = await GetAsync(playerId);
            if (currentPlayer.EndedGames.Any(game => game.Mode == Gamemode.Daily 
                && game.StartDate == DateTimeUtils.UtcNowDateString
            ))
            {
                return true;
            }
            return false;
        }

        public async Task<int> GetBestStreakAsync(string playerId, Gamemode mode)
        {
            Player currentPlayer = await GetAsync(playerId);
            return currentPlayer.GetStats(mode).BestStreak;
        }

        public async Task<int> GetCurrentStreakAsync(string playerId, Gamemode mode) {
            Player currentPlayer = await GetAsync(playerId);
            return currentPlayer.GetStats(mode).CurrentStreak;
        }

        public async Task<GameStatistic> GetPlayerStatsAsync(string playerId, Gamemode mode)
        {
            Player currentPlayer = await GetAsync(playerId);
            GameStatistic stats = currentPlayer.GetStats(mode);
            stats.BestGuess = (await _itemsService.GetAsync(stats.BestGuess)).Name;
            return stats;
        }

        public async Task<PlayerProfile> GetPlayerProfileAsync(string playerId)
        {
            Player currentPlayer = await GetAsync(playerId);
            PlayerProfile playerProfile = new PlayerProfile { 
                Id = currentPlayer.Id,
                Name = currentPlayer.Name,
                RegistrationDate = currentPlayer.RegistrationDate,
                NormalStats = currentPlayer.NormalStats,
                DailyStats = currentPlayer.DailyStats
            };
            playerProfile.NormalStats.BestGuess = (await _itemsService.GetAsync(playerProfile.NormalStats.BestGuess!)).Name;
            playerProfile.DailyStats.BestGuess = (await _itemsService.GetAsync(playerProfile.DailyStats.BestGuess!)).Name;
            return playerProfile;
        }

        public async Task<bool> CreateNewPlayerAsync(string name, string password, string email)
        {
            Player newPlayer = new Player
            {
                Name = name,
                Password = password,
                Email = email,
                NormalStats = new GameStatistic(),
                DailyStats = new GameStatistic(),
                RegistrationDate = DateTimeUtils.UtcNowDateString,
                RegistrationTime = DateTimeUtils.UtcNowTimeString
            };
            if (!await DoesExistAsync(newPlayer))
            {
                await CreateAsync(newPlayer);
                return true;
            }
            else return false;
        }

        public async Task<List<PlayerProfile>> GetDailyLeaderboardAsync()
        {
            List<Player> players = await _playersCollection.Find(player => player.EndedGames
                    .Where(game => game.Mode == Gamemode.Daily
                        && game.StartDate == DateTimeUtils.UtcNowDateString
                        && game.IsEnded
                        && game.GameResult == GameResult.Won)
                    .Any())
                .ToListAsync();
            players = players.OrderByDescending(player => player.EndedGames.First(game => game.Mode == Gamemode.Daily
                        && game.StartDate == DateTimeUtils.UtcNowDateString
                        && game.IsEnded
                        && game.GameResult == GameResult.Won)
                        .GuessItemIds.Count)
                .ThenByDescending(player => player.DailyStats.CurrentStreak)
                .ToList();
            return players.Take(10)
                .Select(player => player
                .ToPlayerProfile(player.EndedGames.First(game => game.Mode == Gamemode.Daily
                        && game.StartDate == DateTimeUtils.UtcNowDateString
                        && game.IsEnded
                        && game.GameResult == GameResult.Won).GuessItemIds.Count))
                .ToList();
        }

        public async Task<List<PlayerProfile>> GetNormalLeaderboardAsync()
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
            return leaderboard.Take(10)
                .Select(player => player
                    .ToPlayerProfile(player.EndedGames
                        .Find(game => game.Mode == Gamemode.Normal
                            && game.IsEnded && game.GameResult == GameResult.Won)!.GuessItemIds.Count))
                .ToList();
        }

        #endregion

        #region private methods

        private async Task CreateKnockOffPlayers(int amount)
        {
            for(int i = 0; i < amount; i++)
            {
                Player player = PlayerExtensions.GetRandomPlayer();
                await CreateAsync(player);
            }
        }

        private async Task CreateAdminAccount()
        {
            await CreateAsync(new Player
            {
                Name = "admin",
                Email = "admin@hotmail.com",
                Password = StringExtensions.GetRandomNameLookingString(),
                RegistrationDate = DateTimeUtils.UtcNowDateString,
                RegistrationTime = DateTimeUtils.UtcNowTimeString,
                NormalStats = GameStatisticExtensions.RandomGameStatistic(),
                DailyStats = GameStatisticExtensions.RandomGameStatistic(),
                EndedGames = GameExtensions.GetListOfRandomGames(),
            });
        }

        #endregion
    }
}
