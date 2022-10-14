using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System.Linq.Expressions;

namespace RomgleWebApi.Extensions
{
    public static class BsonClassMapExtensions
    {
        public static BsonMemberMap MapId<T>(this BsonClassMap<T> map, Expression<Func<T, string>> memberExpression)
        {
            return map.MapIdMember(memberExpression)
                .SetSerializer(new StringSerializer(BsonType.ObjectId));
        }

        public static BsonMemberMap MapDate<T>(this BsonClassMap<T> map, Expression<Func<T, DateTime>> memberExpression)
        {
            return map.MapMember(memberExpression)
                .SetSerializer(new DateTimeSerializer(dateOnly: true));
        }
    }
}
