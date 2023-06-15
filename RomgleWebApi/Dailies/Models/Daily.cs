using MongoDB.Bson;

namespace RotmgleWebApi.Dailies
{
    public class Daily : IExtraElements
    {
        public string Id { get; set; }

        public DateTime StartDate { get; set; }

        public string TargetItemId { get; set; }

        public BsonDocument ExtraElements { get; set; }
    }
}
