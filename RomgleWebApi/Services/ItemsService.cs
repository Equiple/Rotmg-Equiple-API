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

        public async Task<Item?> GetAsync(string id) =>
            await _itemsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Item newItem) =>
            await _itemsCollection.InsertOneAsync(newItem);

        public async Task UpdateAsync(string id, Item updatedItem) =>
            await _itemsCollection.ReplaceOneAsync(x => x.Id == id, updatedItem);

        public async Task RemoveAsync(string id) =>
            await _itemsCollection.DeleteOneAsync(x => x.Id == id);

        public async Task<List<Item>> FindAllAsync(string searchInput)
        {
            if (searchInput == null || searchInput == "")
            {
                return null;
            }

            return await _itemsCollection.Find(x => x.Name.Contains(searchInput)).ToListAsync();
        }
            
        public async Task<Item> GetRandomItemAsync() 
        {
            BsonDocument[] pipeline = new[] { BsonDocument.Parse("{ $sample: {size:1} }") };
            List<Item> randomItem = await _itemsCollection.Aggregate<Item>(pipeline).ToListAsync();
            return randomItem[0];

            //(Item)_itemsCollection.AsQueryable().Sample(1);
        }
            
        
        


    }
}
