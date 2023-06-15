using MongoDB.Bson;

namespace RotmgleWebApi.Complaints
{
    public class Complaint : IExtraElements
    {
        public string Id { get; set; }

        public string Fingerprint { get; set; }

        public string Email { get; set; }

        public DateAndTime Date { get; set; }

        public string Body { get; set; }

        public BsonDocument ExtraElements { get; set; }
    }
}
