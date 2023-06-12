using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RotmgleWebApi.Items;

namespace RotmgleWebApi.Dailies
{
    public class DailyService : IDailyService
    {
        private readonly IMongoCollection<Daily> _dailyCollection;
        private readonly IItemService _itemService;

        public DailyService(
            IOptions<RotmgleDatabaseSettings> rotmgleDatabaseSettings,
            IItemService itemService)
        {
            _dailyCollection = MongoUtils.GetCollection<Daily>(
                rotmgleDatabaseSettings.Value,
                x => x.DailyCollectionName);
            _itemService = itemService;
        }

        public async Task<Daily> GetAsync()
        {
            Daily? daily = await _dailyCollection
                .Find(daily => daily.StartDate == DateTime.UtcNow.Date)
                .FirstOrDefaultAsync();
            if (daily == null)
            {
                Item item = await _itemService.GetRandomItemAsync(reskinsExcluded: false);
                daily = new Daily
                {
                    StartDate = DateTime.UtcNow.Date,
                    TargetItemId = item.Id,
                };
                await _dailyCollection.InsertOneAsync(daily);
            }
            return daily;
        }

        public async Task<int> CountDailiesAsync()
        {
            int count = (int) await _dailyCollection.CountDocumentsAsync(daily => true);
            return count;
        }
    }
}
