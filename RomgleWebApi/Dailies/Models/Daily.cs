using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace RotmgleWebApi.Dailies
{
    public class Daily : IExtraElements
    {
        public string Id { get; set; }

        public DateTime StartDate { get; set; }

        public string TargetItemId { get; set; }

        [JsonIgnore]
        public BsonDocument ExtraElements { get; set; }
    }
}
