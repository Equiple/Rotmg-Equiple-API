﻿using RomgleWebApi.Data.Models;

namespace RomgleWebApi.Data.Extensions
{
    public static class PlayerExtensions
    {
        public static GameStatistic AddWin(this GameStatistic statistic, string guessId, int tries)
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

        public static bool HasCurrentGame(this Player player)
        {
            return player.CurrentGame != null;
        }

        public static bool IsOutOfTries(this Player player)
        {
            return player.CurrentGame.GuessItemIds.Count == 10;
        }

        
    }
}
