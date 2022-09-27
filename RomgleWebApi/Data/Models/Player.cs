using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;

namespace RomgleWebApi.Data.Models
{
    public class Player
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime RegistrationDate { get; set; }
        public GameStatistic NormalStats { get; set; }
        public GameStatistic DailyStats { get; set; } 
        public Game? CurrentGame { get; set; }
        public List<Game> EndedGames { get; set; }
    }
}
