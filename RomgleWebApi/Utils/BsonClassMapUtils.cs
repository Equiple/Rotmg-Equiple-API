using MongoDB.Bson.Serialization;
using RomgleWebApi.Extensions;
using System.Linq.Expressions;

namespace RomgleWebApi.Utils
{
    public static class BsonClassMapUtils
    {
        public static void RegisterIdOnly<T>(Expression<Func<T, string>> memberExpression)
        {
            BsonClassMap.RegisterClassMap<T>(map =>
            {
                map.AutoMap();
                map.MapId(memberExpression);
            });
        }
    }
}
