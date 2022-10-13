using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RomgleWebApi.Data.Models
{
    public class Game
    {
        public Gamemode Mode { get; set; }

        public string TargetItemId { get; set; }

        public List<string> GuessItemIds { get; set; }

        public string StartDate { get; set; } = "";

        public string StartTime { get; set; } = "";

        //public DateTime StartTime { get; set; }

        public bool ReskingExcluded { get; set; }

        public bool IsEnded { get; set; } = false;

        public GameResult? GameResult { get; set; }

    }
}
