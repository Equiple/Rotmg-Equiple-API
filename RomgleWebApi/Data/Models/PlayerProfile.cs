using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace RomgleWebApi.Data.Models
{
    public class PlayerProfile
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]

        public string Id { get; set; }

        public string Name { get; set; }

        public string Role { get; set; }

        public DateAndTime RegistrationDate { get; set; }

        public DetailedGameStatistic NormalStats { get; set; }

        public DetailedGameStatistic DailyStats { get; set; }

        public int DailyGuesses { get; set; }
    }
}
