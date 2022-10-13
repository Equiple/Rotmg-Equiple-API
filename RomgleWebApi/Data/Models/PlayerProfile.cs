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

        public string RegistrationDate { get; set; } = "";

        public string RegistrationTime { get; set; } = "";

        //public DateTime RegistrationDate { get; set; }

        public GameStatistic NormalStats { get; set; }

        public GameStatistic DailyStats { get; set; }

        public int DailyGuesses { get; set; } = 0;

    }
}
