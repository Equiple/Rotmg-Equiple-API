namespace RomgleWebApi.Data
{
    public class RotmgleDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;

        public string ItemsCollectionName { get; set; } = null!;

        public string PlayersCollectionName { get; set; } = null!;

        public string DailiesCollectionName { get; set; } = null!;

    }
}
