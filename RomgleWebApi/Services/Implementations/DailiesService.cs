using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RomgleWebApi.DAL;
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
            IDataCollectionProvider dataCollectionProvider,
            IItemsService itemsService)
        {
            _dailiesCollection = dataCollectionProvider
                .GetDataCollection<Daily>(rotmgleDatabaseSettings.Value.DailiesCollectionName)
                .AsMongo();

            _itemsService = itemsService;
        }

        public async Task<Daily> GetAsync()
        {
            Daily? daily = await _dailiesCollection
                .Find(daily => daily.StartDate == DateTime.UtcNow.Date)
                .FirstOrDefaultAsync();
            if (daily == null)
            {
                Item item = await _itemsService.GetRandomItemAsync(reskinsExcluded: false);
                daily = new Daily
                {
                    StartDate = DateTime.UtcNow.Date,
                    TargetItemId = item.Id
                };
                await _dailiesCollection.InsertOneAsync(daily);
            }
            return daily;
        }
    }
}
