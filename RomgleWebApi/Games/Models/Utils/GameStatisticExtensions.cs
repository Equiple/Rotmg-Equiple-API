using RotmgleWebApi.Items;

namespace RotmgleWebApi.Games
{
    public static class GameStatisticExtensions
    {
        /// <summary>
        /// Returns modified GameStatic based on winning the game.
        /// </summary>
        public static GameStatistic AddWin(this GameStatistic statistic, string guessId, int tries)
        {
            statistic.BestGuessItemId = guessId;
            statistic.RunsWon += 1;
            statistic.CurrentStreak += 1;
            if (statistic.CurrentStreak > statistic.BestStreak)
            {
                statistic.BestStreak = statistic.CurrentStreak;
            }
            if (!statistic.BestRun.HasValue || tries < statistic.BestRun.Value)
            {
                statistic.BestRun = tries;
                statistic.BestGuessItemId = guessId;
            }
            return statistic;
        }

        /// <summary>
        /// Returns modified GameStatic based on losing the game.
        /// </summary>
        public static GameStatistic AddLose(this GameStatistic statistic)
        {
            if (statistic.CurrentStreak > statistic.BestStreak)
            {
                statistic.BestStreak = statistic.CurrentStreak;
            }
            statistic.CurrentStreak = 0;
            statistic.RunsLost += 1;
            return statistic;
        }

        /// <summary>
        /// Converts GameStatistic to DetailedGameStatistic.
        /// </summary>
        public static async Task<GameStatisticDetailed> ToDetailed(
            this GameStatistic stats,
            IItemService itemService)
        {
            Item? item = null;
            if (stats.BestGuessItemId != null)
            {
                item = await itemService.GetAsync(stats.BestGuessItemId);
            }
            GameStatisticDetailed detailedStats = new()
            {
                GameStatistic = stats,
                BestGuessItem = item,
            };  
            return detailedStats;
        }

        /// <summary>
        /// Fills GameStatistic with random values.
        /// </summary>
        public static void Randomize(this GameStatistic stats)
        {
            Random random = new();
            stats.CurrentStreak = random.Next(0, 10);
            stats.BestRun = random.Next(0, 9);
            stats.RunsLost = random.Next(10, 50);
            stats.RunsWon = random.Next(10, 50);
            stats.BestStreak = random.Next(10, 50);
        }
    }
}
