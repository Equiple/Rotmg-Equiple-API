using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace RotmgleWebApi.Items
{
    public class ItemService : IItemService
    {
        private readonly IMongoCollection<Item> _itemCollection;

        public ItemService(IOptions<RotmgleDatabaseSettings> rotmgleDatabaseSettings)
        {
            _itemCollection = MongoUtils.GetCollection<Item>(
                rotmgleDatabaseSettings.Value,
                x => x.ItemCollectionName);
        }

        public async Task<Item> GetAsync(string itemId)
        {
            Item item = await _itemCollection.Find(item => item.Id == itemId).FirstAsync();
            return item;
        }

        public async Task<IEnumerable<Item>> FindAllAsync(string searchInput, bool reskinsExcluded)
        {
            searchInput = searchInput.ToLower();
            List<string> searchTags = searchInput.Split(' ').ToList();
            //List<Item> searchResult = await _itemsCollection.AsQueryable().Where(x => x.Name.ToLower().Contains(searchInput)).ToListAsync();
            IMongoQueryable<Item> searchResult = _itemCollection.AsQueryable();
            if (searchInput == "all")
            {
                searchResult = searchResult.Where(item => true);
            }
            else
            {
                searchResult = searchResult.Where(item => item.Name.ToLower().Contains(searchInput)
                    || searchTags.All(tag => item.Tags.Contains(tag)));
            }
            if (reskinsExcluded)
            {
                searchResult = searchResult.Where(x => !x.Reskin);
            }
            List<Item> result = await searchResult.OrderByDescending(item => item.Name).ToListAsync();
            return result;
        }

        public async Task<Item> GetRandomItemAsync(bool reskinsExcluded)
        {
            IMongoQueryable<Item> itemQuery = _itemCollection.AsQueryable();
            if (reskinsExcluded)
            {
                itemQuery = itemQuery.Where(x => !x.Reskin);
            }
            Item randomItem = await itemQuery.Sample(1).FirstAsync();
            return randomItem;
        }
    }
}
