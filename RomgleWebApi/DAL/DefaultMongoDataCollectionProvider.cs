using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using RomgleWebApi.Data.Settings;

namespace RomgleWebApi.DAL
{
    public class DefaultMongoDataCollectionProvider : IDataCollectionProvider
    {
        private readonly IMongoDatabase _mongoDatabase;

        public DefaultMongoDataCollectionProvider(IOptions<RotmgleDatabaseSettings> rotmgleDatabaseSettings)
        {
            MongoClient mongoClient = new MongoClient(rotmgleDatabaseSettings.Value.ConnectionString);
            _mongoDatabase = mongoClient.GetDatabase(rotmgleDatabaseSettings.Value.DatabaseName);
        }

        public IDataCollection<T> GetDataCollection<T>(string name)
        {
            IMongoCollection<T> mongoCollection = _mongoDatabase.GetCollection<T>(name);
            IDataCollection<T> dataCollection = new DefaultMongoDataCollection<T>(mongoCollection);
            return dataCollection;
        }

        private class DefaultMongoDataCollection<T> : IDataCollection<T>
        {
            private readonly IMongoCollection<T> _mongoCollection;
            private readonly IMongoQueryable<T> _mongoQueryable;

            public DefaultMongoDataCollection(IMongoCollection<T> mongoCollection)
            {
                _mongoCollection = mongoCollection;
                _mongoQueryable = _mongoCollection.AsQueryable();
            }

            public IQueryable<T> AsQueryable()
            {
                return _mongoQueryable;
            }

            public IMongoCollection<T> AsMongo()
            {
                return _mongoCollection;
            }
        }
    }
}
