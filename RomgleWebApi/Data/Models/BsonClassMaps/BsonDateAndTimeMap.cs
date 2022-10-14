using MongoDB.Bson.Serialization;
using RomgleWebApi.Extensions;

namespace RomgleWebApi.Data.Models.BsonClassMaps
{
    public static class BsonDateAndTimeMap
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
