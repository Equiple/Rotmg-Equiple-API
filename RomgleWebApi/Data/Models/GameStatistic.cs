namespace RomgleWebApi.Data.Models
{
    public class GameStatistic
    {
        public int BestRun { get; set; } = 10;

        public string? BestGuess { get; set; } = "";//

        public int RunsLost { get; set; } = 0;

        public int RunsWon { get; set; } = 0;

        public int CurrentStreak { get; set; } = 0;

        public int BestStreak { get; set; } = 0;

    }
}
