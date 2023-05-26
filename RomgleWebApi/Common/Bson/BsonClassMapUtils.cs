using MongoDB.Bson.Serialization;
using System.Linq.Expressions;

namespace RotmgleWebApi
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
