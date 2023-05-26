using MongoDB.Driver;

namespace RotmgleWebApi
{
    public static class MongoUtils
    {
        public static IMongoCollection<T> GetCollection<T>(
            RotmgleDatabaseSettings settings,
            Func<RotmgleDatabaseSettings, string> collectionName)
        {
            MongoClient client = new(settings.ConnectionString);
            IMongoDatabase db = client.GetDatabase(settings.DatabaseName);
            string colName = collectionName.Invoke(settings);
            IMongoCollection<T> collection = db.GetCollection<T>(colName);
            return collection;
        }
    }
}
