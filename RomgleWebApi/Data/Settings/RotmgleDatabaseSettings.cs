namespace RomgleWebApi.Data.Settings
{
    public class RotmgleDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;

        public string ItemCollectionName { get; set; } = null!;

        public string PlayerCollectionName { get; set; } = null!;

        public string DailyCollectionName { get; set; } = null!;

        public string RefreshTokenCollectionName { get; set; } = null!;

        public string ComplaintCollectionName { get; set; } = null!;
    }
}
