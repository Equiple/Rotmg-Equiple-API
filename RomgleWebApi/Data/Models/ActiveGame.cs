using MongoDB.Bson.Serialization.Attributes;

namespace RomgleWebApi.Data.Models
{
    public class ActiveGame
    {
        public string Gamemode { get; set; }
        public string TargetItemId { get; set; }
        public List<string> GuessItemIds { get; set; }
        [BsonDateTimeOptions]
        public DateTime StartTime { get; set; }
    }
}
