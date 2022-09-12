using RomgleWebApi.Data.Models;

namespace RomgleWebApi.Data.Extensions
{
    public static class PlayerExtensions
    {
        public static void AddWin(this GameStatistic statistic, string guessId, int tries)
        {
            statistic.BestGuess = guessId;
            statistic.RunsWon += 1;
            statistic.CurrentStreak += 1;
            if (statistic.CurrentStreak > statistic.BestStreak)
            {
                statistic.BestStreak = statistic.CurrentStreak;
            }
            if (tries < statistic.BestRun)
            {
                statistic.BestRun = tries;
                statistic.BestGuess = guessId;
            }
        }

        public static void AddLose(this GameStatistic statistic)
        {
            if (statistic.CurrentStreak > statistic.BestStreak)
            {
                statistic.BestStreak = statistic.CurrentStreak;
            }
            statistic.CurrentStreak = 0;
            statistic.RunsLost += 1;
        }

        public static void UpdatePlayerScore(this Player player, bool positive)
        {
            if(player.CurrentGame.Gamemode == "Normal")
            {
                if (positive)
                {
                    player.NormalStats.AddWin(player.CurrentGame.TargetItemId, player.CurrentGame.GuessItemIds.Count);
                }
                else
                {
                    player.NormalStats.AddLose();
                }
            } 
            else if (player.CurrentGame.Gamemode == "Daily")
            {
                if (positive)
                {
                    player.DailyStats.AddWin(player.CurrentGame.TargetItemId, player.CurrentGame.GuessItemIds.Count);
                }
                else
                {
                    player.DailyStats.AddLose();
                }
                player.DailyAttempted = true;
            }
            player.CurrentGame = null;
        }

        public static bool CheckCurrentGame(this Player player, string itemId)
        {
            player.CurrentGame.GuessItemIds.Append(itemId);
            return player.CurrentGame.TargetItemId == itemId;
        }

        public static bool HasCurrentGame(this Player player)
        {
            return player.CurrentGame != null;
        }

        public static bool IsOutOfTries(this Player player)
        {
            return player.CurrentGame.GuessItemIds.Count == 10;
        }

        public static void StartNewGame(this Player player, string targetItemId, string gamemode)
        {
            player.CurrentGame = new ActiveGame { 
                StartTime= DateTime.Now, 
                TargetItemId= targetItemId,
                GuessItemIds= new List<string>(),
                Gamemode= gamemode
            };
        }
    }
}
