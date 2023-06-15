using MongoDB.Bson;

namespace RotmgleWebApi
{
    public interface IExtraElements
    {
        BsonDocument ExtraElements { get; set; }
    }
}
