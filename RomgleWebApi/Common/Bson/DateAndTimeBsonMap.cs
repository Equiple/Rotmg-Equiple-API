using MongoDB.Bson.Serialization;

namespace RotmgleWebApi
{
    public static class DateAndTimeBsonMap
    {
        public static void Register()
        {
            BsonClassMap.RegisterClassMap<DateAndTime>(map =>
            {
                map.AutoMap();
                map.MapDate(dateAndTime => dateAndTime.Date);
            });
        }
    }
}
