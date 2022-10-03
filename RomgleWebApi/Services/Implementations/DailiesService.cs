using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Settings;

namespace RomgleWebApi.Services.Implementations
{
    public class DailiesService : IDailiesService
    {
        private readonly IMongoCollection<Daily> _dailiesCollection;
        private readonly IItemsService _itemsService;

        public DailiesService(
            IOptions<RotmgleDatabaseSettings> rotmgleDatabaseSettings,
            IItemsService itemsService)
        {
            MongoClient mongoClient = new MongoClient(
                rotmgleDatabaseSettings.Value.ConnectionString);

            IMongoDatabase mongoDatabase = mongoClient.GetDatabase(
                rotmgleDatabaseSettings.Value.DatabaseName);

            _dailiesCollection = mongoDatabase.GetCollection<Daily>(
                rotmgleDatabaseSettings.Value.DailiesCollectionName);

            _itemsService = itemsService;
        }

        //Daily - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - - 
        //
        public async Task<Daily> GetAsync() =>
            await _dailiesCollection.Find(x => x.Timestamp == DateTime.UtcNow.Date).FirstOrDefaultAsync();

        public async Task<Daily> GetAsync(string id) =>
            await _dailiesCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task<Daily?> GetAsync(DateTime timestamp) =>
            await _dailiesCollection.Find(x => x.Timestamp == timestamp).FirstOrDefaultAsync();

        private async Task<List<Daily>> GetAllAsync() =>
            await _dailiesCollection.Find(_ => true).ToListAsync();

        private async Task CreateAsync(Daily newDaily) =>
            await _dailiesCollection.InsertOneAsync(newDaily);

        public async Task CreateAsync(string itemId) =>
            await _dailiesCollection.InsertOneAsync(new Daily { Timestamp = DateTime.UtcNow.Date, TargetItemId = itemId });

        private async Task UpdateAsync(string id, Daily updatedItem) =>
            await _dailiesCollection.ReplaceOneAsync(x => x.Id == id, updatedItem);

        private async Task RemoveAsync(string id) =>
            await _dailiesCollection.DeleteOneAsync(x => x.Id == id);

        public async Task<Daily> GetDailyItem() 
        {
            Daily today = await GetAsync(DateTime.UtcNow.Date);
            if (today is null)
            {
                Item itm = await _itemsService.GetRandomItemAsync(false);
                await CreateAsync(itm.Id);
                today = await GetAsync(itm.Id);
            }
            return today;
        }

        public async Task<bool> CheckDailyItem(string itemId)
        {
            Daily currentDaily = await GetAsync();
            if (currentDaily.TargetItemId == itemId)
            {
                return true;
            }
            else return false;
        }
            

    }
}

