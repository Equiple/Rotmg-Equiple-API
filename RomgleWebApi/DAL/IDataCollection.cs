using MongoDB.Driver;

namespace RomgleWebApi.DAL
{
    public interface IDataCollection<T>
    {
        IQueryable<T> AsQueryable();

        IMongoCollection<T> AsMongo();
    }
}
