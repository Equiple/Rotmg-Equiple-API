using MongoDB.Driver;

namespace RotmgleWebApi
{
    public static class MongoUtils
    {
        public static IMongoCollection<T> GetCollection<T>(
            IMongoDatabaseOptions options,
            string collectionName)
        {
            MongoClient client = new(options.ConnectionString);
            IMongoDatabase db = client.GetDatabase(options.DatabaseName);
            IMongoCollection<T> collection = db.GetCollection<T>(collectionName);
            return collection;
        }
    }
}
