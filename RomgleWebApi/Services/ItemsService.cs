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

        public async Task<IEnumerable<Item>> FindAllAsync(string searchInput, bool reskinsExcluded)
        {
            searchInput = searchInput.ToLower();
            //List<Item> searchResult = await _itemsCollection.AsQueryable().Where(x => x.Name.ToLower().Contains(searchInput)).ToListAsync();
            IMongoQueryable<Item> searchResult = _itemsCollection.AsQueryable().Where(x => x.Name.ToLower().Contains(searchInput));
            if (reskinsExcluded)
            {
                searchResult = searchResult.Where(x => !x.Reskin);
            }
            return await searchResult.OrderByDescending(item => item.Name).ToListAsync();
        }
            
        public async Task<Item> GetRandomItemAsync(bool reskinsExcluded) 
        {
            //List<BsonDocument> pipeline = new List<BsonDocument>();
            IMongoQueryable<Item> randomItem;
            if (reskinsExcluded)
            {
                randomItem = _itemsCollection.AsQueryable().Where(x => !x.Reskin);
                //pipeline.Add(BsonDocument.Parse("{ $match: { reskin:false} }"));
            } else
            {
                randomItem = _itemsCollection.AsQueryable();
            }
            Item itm = randomItem.Sample(1).First();
            //pipeline.Add(BsonDocument.Parse("{ $sample: {size: 1} }"));
            //List<Item> randomItem = await _itemsCollection.Aggregate<Item>(pipeline).ToListAsync();
            if (itm == null)
            {
                throw new Exception("Query returned empty list.");
            }
            //return randomItem[0];
            return itm;
        }
    }
}
