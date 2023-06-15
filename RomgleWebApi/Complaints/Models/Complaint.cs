using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace RotmgleWebApi.Complaints
{
    public class Complaint : IExtraElements
    {
        public string Id { get; set; }

        public string Fingerprint { get; set; }

        public string Email { get; set; }

        public DateAndTime Date { get; set; }

        public string Body { get; set; }

        [JsonIgnore]
        public BsonDocument ExtraElements { get; set; }
    }
}
