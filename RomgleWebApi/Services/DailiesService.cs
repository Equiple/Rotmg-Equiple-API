using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RomgleWebApi.Data;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Utils;

namespace RomgleWebApi.Services
{
    public class DailiesService
    {
        private readonly IMongoCollection<Daily> _dailiesCollection;
        private readonly ItemsService _itemsService;

        public DailiesService(
            IOptions<RotmgleDatabaseSettings> rotmgleDatabaseSettings, ItemsService itemsService)
        {
            MongoClient mongoClient = new MongoClient(
                rotmgleDatabaseSettings.Value.ConnectionString);

            IMongoDatabase mongoDatabase = mongoClient.GetDatabase(
                rotmgleDatabaseSettings.Value.DatabaseName);

            _dailiesCollection = mongoDatabase.GetCollection<Daily>(
                rotmgleDatabaseSettings.Value.DailiesCollectionName);

            _itemsService = itemsService;
        }

        #region public methods

        public async Task<Daily> GetAsync() =>
            await _dailiesCollection.Find(x => x.StartDate == DateTimeUtils.UtcNowDateString).FirstOrDefaultAsync();

        public async Task<Daily> GetAsync(string timestamp) =>
            await _dailiesCollection.Find(x => x.StartDate == timestamp).FirstAsync();

        public async Task CreateAsync(string itemId)
        {
            await _dailiesCollection.InsertOneAsync(new Daily
            {
                StartDate = DateTimeUtils.UtcNowDateString,
                TargetItemId = itemId
            });
        }

        public async Task<Daily> GetDailyItem() 
        {
            bool doesExist = await DoesExist(DateTimeUtils.UtcNowDateString);
            if (!doesExist)
            {
                var itemId = _itemsService.GetRandomItem(reskinsExcluded: false).Id;
                await CreateAsync(itemId);
            }
            Daily today = await GetAsync(DateTimeUtils.UtcNowDateString);
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

        #endregion

        #region private methods

        private async Task<bool> DoesExist(string timestamp)
        {
            return await _dailiesCollection.Find(daily => daily.StartDate == timestamp).AnyAsync();
        }

        #endregion
    }
}

