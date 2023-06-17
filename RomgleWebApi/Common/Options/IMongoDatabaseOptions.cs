namespace RotmgleWebApi
{
    public interface IMongoDatabaseOptions
    {
        string ConnectionString { get; }

        string DatabaseName { get; }
    }
}
