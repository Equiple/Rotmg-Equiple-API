using Hangfire;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using RomgleWebApi.DAL;
using RomgleWebApi.Data;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Models.Auth;
using RomgleWebApi.Data.Settings;
using RomgleWebApi.Extensions;
using RomgleWebApi.Utils;
using System.Text;

namespace RomgleWebApi.Services.Implementations
{
    public class PlayerService : IPlayerService
    {
        private readonly TokenAuthorizationSettings _tokenAuthorizationSettings;
        private readonly IMongoCollection<Player> _playersCollection;
        private readonly IItemService _itemsService;

        public PlayerService(
            IOptions<RotmgleDatabaseSettings> rotmgleDatabaseSettings,
            IOptions<TokenAuthorizationSettings> tokenAuthorizationSettings,
            IDataCollectionProvider dataCollectionProvider,
            IItemService itemsService)
        {
            _tokenAuthorizationSettings = tokenAuthorizationSettings.Value;
            _playersCollection = dataCollectionProvider
                .GetDataCollection<Player>(rotmgleDatabaseSettings.Value.PlayerCollectionName)
                .AsMongo();
            _itemsService = itemsService;
        }

        public async Task<Player> GetAsync(string playerId)
        {
            Player player = await _playersCollection.Find(player => player.Id == playerId).FirstAsync();
            return player;
        }

        public async Task InvalidateExpiredDailyGamesAsync()
        { 
            IMongoQueryable<Player> players = _playersCollection
                .AsQueryable()
                .Where(player => player.CurrentGame != null
                    && !player.CurrentGame.IsEnded
                    && player.CurrentGame.Mode == Gamemode.Daily
                );
            foreach(Player player in players)
            {
                await UpdatePlayerScoreAsync(player, GameResult.Lost);
            }
        }

        public async Task RemoveInactiveGuestAccountsAsync()
        {
            DateTime weekAgo = DateTime.UtcNow.Date.AddDays(-7);
            await _playersCollection.DeleteManyAsync(player => player.Identities[0].Provider == IdentityProvider.Self
                    && player.LastSeen.Date < weekAgo);
        }

        public async Task<PlayerByIdentity?> GetByIdentityAsync(Identity identity)
        {
            Player? player = await _playersCollection
                .Find(player => player.Identities
                    .Where(playerIdentity => playerIdentity.Provider == identity.Provider
                        && playerIdentity.Id == identity.Id)
                    .Any())
                .FirstOrDefaultAsync();
            if (player == null)
            {
                return null;
            }
            PlayerByIdentity result = new PlayerByIdentity(
                player,
                player.Identities
                    .First(playerIdentity => playerIdentity.Provider == identity.Provider
                        && playerIdentity.Id == identity.Id));
            return result;
        }

        public async Task<NewPlayer> CreateNewAsync(Identity identity, string? name = null)
        {
            PlayerByIdentity? existingPlayer = await GetByIdentityAsync(identity);
            if (existingPlayer.HasValue)
            {
                throw new Exception($"Player with given identity {identity.Provider}:{identity.Id} already exists");
            }

            name ??= StringUtils.GetRandomDefaultName();
            string deviceId = await GenerateUniqueDeviceId();
            string personalKey = await GenerateUniquePersonalKey();
            NewPlayer newPlayer = PlayerUtils.Create(identity, name, deviceId, personalKey);
            await _playersCollection.InsertOneAsync(newPlayer.Player);

            return newPlayer;
        }

        public async Task<Device> CreateNewDeviceAsync(string playerId)
        {
            Player player = await GetAsync(playerId);
            string deviceId = await GenerateUniqueDeviceId();
            string personalKey = await GenerateUniquePersonalKey();
            Device newDevice = DeviceUtils.Create(deviceId, personalKey);
            player.Devices.Add(newDevice);
            await UpdateAsync(player);

            return newDevice;
        }

        public async Task UpdateAsync(Player updatedPlayer)
        {
            updatedPlayer.LastSeen = DateTime.UtcNow;
            await _playersCollection.ReplaceOneAsync(player => player.Id == updatedPlayer.Id, updatedPlayer);
        }

        public async Task RefreshPersonalKeyAsync(string playerId, string deviceId)
        {
            Player player = await GetAsync(playerId);
            Device device = player.GetDevice(deviceId);
            string key = await GenerateUniquePersonalKey();
            device.PersonalKey = key;
            await UpdateAsync(player);
        }

        public async Task<bool> WasDailyAttemptedAsync(string id)
        {
            Player player = await GetAsync(id);
            if (player.EndedGames
                    .Where(game => game.Mode == Gamemode.Daily
                        && game.StartDate.Date == DateTime.UtcNow.Date)
                    .Any())
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

        public async Task UpdatePlayerScoreAsync(Player player, GameResult result)
        {
            if (player.CurrentGame == null)
            {
                return;
            }
            if (player.CurrentGame.Mode == Gamemode.Normal)
            {
                if (result == GameResult.Won)
                {
                    player.NormalStats = player.NormalStats.AddWin(player.CurrentGame.TargetItemId, player.CurrentGame.GuessItemIds.Count);
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
                    player.DailyStats = player.DailyStats.AddWin(player.CurrentGame.TargetItemId, player.CurrentGame.GuessItemIds.Count);
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

        #region private methods

        private async Task<string> GenerateUniqueDeviceId()
        {
            string id;
            bool alreadyExists;
            do
            {
                id = DeviceUtils.GenerateDeviceId();
                Player? existingPlayer = await _playersCollection
                    .Find(player => player.Devices
                        .Where(device => device.Id == id)
                        .Any())
                    .FirstOrDefaultAsync();
                alreadyExists = existingPlayer != null;
            }
            while (alreadyExists);
            return id;
        }

        private async Task<string> GenerateUniquePersonalKey()
        {
            const int minSecurityKeyByteCount = 64;
            const int minPersonalKeyByteCount = 32;
            int secretKeyByteCount = Encoding.GetEncoding(_tokenAuthorizationSettings.SecretKeyEncoding)
                .GetByteCount(_tokenAuthorizationSettings.SecretKey);
            int personalKeyByteCount = Math.Max(
                minSecurityKeyByteCount - secretKeyByteCount,
                minPersonalKeyByteCount);

            string key;
            bool alreadyExists;
            do
            {
                key = SecurityUtils.GenerateBase64SecurityKey(personalKeyByteCount);
                Player? existingPlayer = await _playersCollection
                    .Find(player => player.Devices
                        .Where(device => device.PersonalKey == key)
                        .Any())
                    .FirstOrDefaultAsync();
                alreadyExists = existingPlayer != null;
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
                    Id = "knock_off"
                };
                NewPlayer newPlayer = await CreateNewAsync(
                    identity,
                    name: StringUtils.GenerateRandomNameLookingString());
                Player player = newPlayer.Player;
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
            player[0].Role = "admin";
            await UpdateAsync(player[0]);
        }

        #endregion private methods
    }
}
