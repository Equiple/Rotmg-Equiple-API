using RomgleWebApi.Data;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Services;

namespace RomgleWebApi.Extensions
{
    public static class GameStatisticExtensions
    {
        public static GameStatistic AddWin(this GameStatistic statistic, string guessId, int tries)
        {
            statistic.BestGuessItemId = guessId;
            statistic.RunsWon += 1;
            statistic.CurrentStreak += 1;
            if (statistic.CurrentStreak > statistic.BestStreak)
            {
                statistic.BestStreak = statistic.CurrentStreak;
            }
            if (tries < statistic.BestRun)
            {
                statistic.BestRun = tries;
                statistic.BestGuessItemId = guessId;
            }
            return statistic;
        }

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

        public static Task<DetailedGameStatistic> ToDetailed(
            this GameStatistic stats,
            IItemsService itemsService)
        {
            return stats.ToDetailed(getters: new DetailedGameStatisticGetters
            {
                BestGuessItem = itemId => itemsService.GetAsync(itemId)
            });
        }

        public static async Task<DetailedGameStatistic> ToDetailed(
            this GameStatistic stats,
            DetailedGameStatisticGetters getters = default)
        {
            DetailedGameStatistic detailedStats = new DetailedGameStatistic
            {
                GameStatistic = stats,
                BestGuessItem = stats.BestGuessItemId == null || getters.BestGuessItem == null
                    ? null
                    : await getters.BestGuessItem.Invoke(stats.BestGuessItemId)
            };
            return detailedStats;
        }

        /// <summary>
        /// Creates a GameStatistic with random values.
        /// </summary>
        /// <returns>
        /// Random GameStatistic.
        /// </returns>
        public static void Randomize(this GameStatistic stats)
        {
            Random random = new Random();
            stats.CurrentStreak = random.Next(0, 10);
            stats.BestRun = random.Next(0, 9);
            stats.RunsLost = random.Next(10, 50);
            stats.RunsWon = random.Next(10, 50);
            stats.BestStreak = random.Next(10, 50);
        }
    }
}
