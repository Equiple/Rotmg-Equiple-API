using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.Data.Models
{
    public class Player
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public List<Identity> Identities { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; }
        public string SecretKey { get; set; }
        public DateTime RegistrationDate { get; set; }
        public GameStatistic NormalStats { get; set; }
        public GameStatistic DailyStats { get; set; }
        public Game? CurrentGame { get; set; }
        public List<Game> EndedGames { get; set; }
    }
}
