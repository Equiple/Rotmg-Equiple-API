using MongoDB.Bson.Serialization;

namespace RotmgleWebApi.AuthenticationImplementation
{
    public static class SessionBsonMap
    {
        public static void Register()
        {
            BsonClassMap.RegisterClassMap<Session>(map =>
            {
                map.AutoMap();
                map.MapIdMember(session => session.AccessToken);
            });
        }
    }
}
