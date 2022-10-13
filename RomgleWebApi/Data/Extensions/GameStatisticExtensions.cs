using RomgleWebApi.Data.Models;

namespace RomgleWebApi.Data.Extensions
{
    public static class GameStatisticExtensions
    {
        /// <summary>
        /// Creates a GameStatistic with random values.
        /// </summary>
        /// <returns>
        /// Random GameStatistic.
        /// </returns>
        public static GameStatistic RandomGameStatistic()
        {
            Random random = new Random();
            return new GameStatistic
            {
                CurrentStreak = random.Next(0, 10),
                BestRun = random.Next(0, 9),
                RunsLost = random.Next(10, 50),
                RunsWon = random.Next(10, 50),
                BestStreak = random.Next(10, 50)
            };
        }
    }
}
