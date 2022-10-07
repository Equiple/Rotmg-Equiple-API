using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using RomgleWebApi.DAL;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Models.Auth;
using RomgleWebApi.Data.Settings;

namespace RomgleWebApi.Services.Implementations
{
    public class PlayersService : IPlayersService
    {
        private readonly IMongoCollection<Player> _playersCollection;

        public PlayersService(
            IOptions<RotmgleDatabaseSettings> rotmgleDatabaseSettings,
            IDataCollectionProvider dataCollectionProvider)
        {
            _playersCollection = dataCollectionProvider
                .GetDataCollection<Player>(rotmgleDatabaseSettings.Value.PlayersCollectionName)
                .AsMongo();
        }

        public async Task<Player> GetAsync(string id)
        {
            Player player = await _playersCollection.Find(player => player.Id == id).FirstAsync();
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

        public async Task UpdateAsync(Player updatedPlayer)
        {
            await _playersCollection.ReplaceOneAsync(player => player.Id == updatedPlayer.Id, updatedPlayer);
        }

        public async Task<bool> WasDailyAttemptedAsync(string id)
        {
            Player currentPlayer = await GetAsync(id);
            if (currentPlayer.EndedGames.Select(game => game.StartTime.Date == DateTime.Now.Date).Any())
            {
                return true;
            }
            return false;
        }

        public async Task<Player> CreateNewPlayerAsync(Identity identity)
        {
            Player? existingPlayer = await GetByIdentityAsync(identity);
            if (existingPlayer != null)
            {
                throw new Exception($"Player with given identity {identity.Provider}:{identity.Id} already exists");
            }

            Player newPlayer = new Player
            {
                Identities = new List<Identity> { identity },
                RefreshTokens = new List<RefreshToken>(),
                NormalStats = new GameStatistic(),
                DailyStats = new GameStatistic(),
                RegistrationDate = DateTime.UtcNow
            };
            await _playersCollection.InsertOneAsync(newPlayer);

            return newPlayer;
        }
    }
}
