namespace RotmgleWebApi.Games
{
    public class GameStatistic
    {
        public int? BestRun { get; set; }

        public string? BestGuessItemId { get; set; }

        public int RunsLost { get; set; }

        public int RunsWon { get; set; }

        public int CurrentStreak { get; set; }

        public int BestStreak { get; set; }
    }
}
