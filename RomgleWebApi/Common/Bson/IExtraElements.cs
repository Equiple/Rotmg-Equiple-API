using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace RotmgleWebApi
{
    public interface IExtraElements
    {
        // TODO: probably shouldn't use attributes in model, also needed to add attribute in each model
        [JsonIgnore]
        BsonDocument ExtraElements { get; set; }
    }
}
