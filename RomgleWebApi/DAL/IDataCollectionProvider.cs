namespace RomgleWebApi.DAL
{
    public interface IDataCollectionProvider
    {
        IDataCollection<T> GetDataCollection<T>(string name);
    }
}
