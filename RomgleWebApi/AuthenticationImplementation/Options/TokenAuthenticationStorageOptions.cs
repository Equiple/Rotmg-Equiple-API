namespace RotmgleWebApi.AuthenticationImplementation
{
    public class TokenAuthenticationStorageOptions : IMongoDatabaseOptions
    {
        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }

        public string SessionCollectionName { get; set; }
    }
}
