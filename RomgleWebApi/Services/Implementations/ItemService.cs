using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using RomgleWebApi.DAL;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Settings;

namespace RomgleWebApi.Services.Implementations
{
    public class ItemService : IItemService
    {
        private readonly IMongoCollection<Item> _itemsCollection;

        public ItemService(
            IOptions<RotmgleDatabaseSettings> rotmgleDatabaseSettings,
            IDataCollectionProvider dataCollectionProvider)
        {
            _itemsCollection = dataCollectionProvider
                .GetDataCollection<Item>(rotmgleDatabaseSettings.Value.ItemCollectionName)
                .AsMongo();
        }

        public async Task<Item> GetAsync(string itemId) 
        {
            return await _itemsCollection.Find(item => item.Id == itemId).FirstAsync();
        }

        public async Task<IReadOnlyList<Item>> FindAllAsync(string searchInput, bool reskinsExcluded)
        {
            searchInput = searchInput.ToLower();
            List<string> searchTags = searchInput.Split(' ').ToList();
            //List<Item> searchResult = await _itemsCollection.AsQueryable().Where(x => x.Name.ToLower().Contains(searchInput)).ToListAsync();
            IMongoQueryable<Item> searchResult;
            if (searchInput == "all")
            {
                searchResult = _itemsCollection.AsQueryable().Where(item => true);
            }
            else
            {
                searchResult = _itemsCollection.AsQueryable().Where(
                    item => item.Name.ToLower().Contains(searchInput) || searchTags.All(tag => item.Tags.Contains(tag)));
            }
            if (reskinsExcluded)
            {
                searchResult = searchResult.Where(x => !x.Reskin);
            }
            return await searchResult.OrderByDescending(item => item.Name).ToListAsync();
        }

        public async Task<Item> GetRandomItemAsync(bool reskinsExcluded)
        {
            IMongoQueryable<Item> itemQuery = _itemsCollection.AsQueryable();
            if (reskinsExcluded)
            {
                itemQuery = itemQuery.Where(x => !x.Reskin);
            }
            Item randomItem = await itemQuery.Sample(1).FirstAsync();
            return randomItem;
        }
    }
}
