using MongoDB.Bson.Serialization;

namespace RotmgleWebApi.Dailies
{
    public static class BsonDailyMap
    {
        public static void Register()
        {
            BsonClassMap.RegisterClassMap<Daily>(map =>
            {
                map.AutoMap();
                map.MapId(daily => daily.Id);
                map.MapDate(daily => daily.StartDate);
            });
        }
    }
}
