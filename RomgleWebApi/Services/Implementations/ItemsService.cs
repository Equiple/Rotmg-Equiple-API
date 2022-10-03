using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Settings;

namespace RomgleWebApi.Services.Implementations
{
    public class ItemsService : IItemsService
    {
        private readonly IMongoCollection<Item> _itemsCollection;

        public ItemsService(
            IOptions<RotmgleDatabaseSettings> rotmgleDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                rotmgleDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                rotmgleDatabaseSettings.Value.DatabaseName);

            _itemsCollection = mongoDatabase.GetCollection<Item>(
                rotmgleDatabaseSettings.Value.ItemsCollectionName);
        }

        public async Task<Item> GetAsync(string id) 
        {
            return await _itemsCollection.Find(x => x.Id == id).FirstAsync();
        }

        public async Task<IReadOnlyList<Item>> FindAllAsync(string searchInput, bool reskinsExcluded)
        {
            searchInput = searchInput.ToLower();
            List<string> searchTags = searchInput.Split(' ').ToList();
            //List<Item> searchResult = await _itemsCollection.AsQueryable().Where(x => x.Name.ToLower().Contains(searchInput)).ToListAsync();
            IMongoQueryable<Item> searchResult = _itemsCollection.AsQueryable().Where(
                item => item.Name.ToLower().Contains(searchInput) || searchTags.All(tag => item.Tags.Contains(tag)));
            if (reskinsExcluded)
            {
                searchResult = searchResult.Where(x => !x.Reskin);
            }
            return await searchResult.OrderByDescending(item => item.Name).ToListAsync();
        }

        public async Task<Item> GetRandomItemAsync(bool reskinsExcluded)
        {
            IMongoQueryable<Item> randomItem = itemsCollection.AsQueryable();
            if (reskinsExcluded)
            {
                randomItem = _randomItem.Where(x => !x.Reskin);
            }
            Item randomItem = await itemQuery.Sample(1).FirstAsync();
            return randomItem;
        }
    }
}
