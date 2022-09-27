using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RomgleWebApi.Data;
using RomgleWebApi.Data.Extensions;
using RomgleWebApi.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace RomgleWebApi.Services
{
    public class PlayersService
    {
        private readonly IMongoCollection<Player> _playersCollection;

        public PlayersService(
            IOptions<RotmgleDatabaseSettings> rotmgleDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                rotmgleDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                rotmgleDatabaseSettings.Value.DatabaseName);

            _playersCollection = mongoDatabase.GetCollection<Player>(
                rotmgleDatabaseSettings.Value.PlayersCollectionName);
        }

        //Players  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - - 
        //
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
            if (currentPlayer.EndedGames.Select(game => game.StartTime.Date == DateTime.Now.Date).Any())
            {
                return true;
            }
            return false;
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
                RegistrationDate = DateTime.Now
            };
            if (!await DoesExistAsync(newPlayer))
            {
                await CreateAsync(newPlayer);
                return true;
            }
            else return false;
        }
        

    }
}
