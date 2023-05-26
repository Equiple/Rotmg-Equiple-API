namespace RotmgleWebApi.Games
{
    public class Game
    {
        public Gamemode Mode { get; set; }

        public string TargetItemId { get; set; }

        public List<string> GuessItemIds { get; set; }

        public DateAndTime StartDate { get; set; }

        public bool ReskingExcluded { get; set; }

        public bool IsEnded { get; set; }

        public GameResult? GameResult { get; set; }
    }
}
