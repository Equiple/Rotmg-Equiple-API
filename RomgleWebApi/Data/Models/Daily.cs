using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RomgleWebApi.Data.Models
{
    public class Daily
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string TargetItemId { get; set; }
    }
}
