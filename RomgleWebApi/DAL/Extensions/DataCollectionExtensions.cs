using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace RomgleWebApi.DAL.Extensions
{
    public static class DataCollectionExtensions
    {
        public static IMongoQueryable<T> AsMongoQueryable<T>(this IDataCollection<T> dataCollection)
        {
            return dataCollection.AsMongo().AsQueryable();
        }
    }
}
