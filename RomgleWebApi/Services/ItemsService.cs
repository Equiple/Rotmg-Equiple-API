using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using RomgleWebApi.Data;
using RomgleWebApi.Data.Models;

namespace RomgleWebApi.Services
{
    public class ItemsService
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

        //Item  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - - 
        //
        public async Task<List<Item>> GetAsync() =>
            await _itemsCollection.Find(_ => true).ToListAsync();

        public async Task<Item> GetAsync(string id) 
        {
            return await _itemsCollection.Find(x => x.Id == id).FirstAsync();
        }

        public async Task CreateAsync(Item newItem) =>
            await _itemsCollection.InsertOneAsync(newItem);

        public async Task UpdateAsync(string id, Item updatedItem) =>
            await _itemsCollection.ReplaceOneAsync(x => x.Id == id, updatedItem);

        public async Task RemoveAsync(string id) =>
            await _itemsCollection.DeleteOneAsync(x => x.Id == id);

        public async Task<IEnumerable<Item>> FindAllAsync(string searchInput, bool reskinsExcluded)
        {
            searchInput = searchInput.ToLower();
            //List<Item> searchResult = await _itemsCollection.AsQueryable().Where(x => x.Name.ToLower().Contains(searchInput)).ToListAsync();
            IMongoQueryable<Item> searchResult = _itemsCollection.AsQueryable().Where(
                x => x.Name.ToLower().Contains(searchInput) || x.Tags.Contains(searchInput));
            if (reskinsExcluded)
            {
                searchResult = searchResult.Where(x => !x.Reskin);
            }
            return await searchResult.OrderByDescending(item => item.Name).ToListAsync();
        }
            
        public Item GetRandomItem(bool reskinsExcluded) 
        {
            IMongoQueryable<Item> itemQuery;
            if (reskinsExcluded)
            {
                itemQuery = _itemsCollection.AsQueryable().Where(x => !x.Reskin);
            } 
            else
            {
                itemQuery = _itemsCollection.AsQueryable();
            }
            Item randomItem = itemQuery.Sample(1).First();
            return randomItem;
        }
    }
}
