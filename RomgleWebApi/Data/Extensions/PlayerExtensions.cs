using RomgleWebApi.Data.Models;
using RomgleWebApi.Utils;

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

        public static GameStatistic GetStats(this Player player, Gamemode mode)
        {
            if (mode == Gamemode.Normal)
            {
                return player.NormalStats;
            }
            else if (mode == Gamemode.Daily)
            {
                return player.DailyStats;
            }
            else throw new Exception($"Exception at GetStats(), PlayerExtensions class: Could not find statistic for {mode} gamemode.\n");
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

        public static bool HasActiveGame(this Player player)
        {
            return player.CurrentGame != null && !player.CurrentGame.IsEnded;
        }

        public static bool IsOutOfTries(this Player player)
        {
            return player.CurrentGame != null && player.CurrentGame.GuessItemIds.Count == 10;
        }

        public static async Task<List<PlayerProfile>> ToPlayerProfile(this List<Player> players)
        {
            List<PlayerProfile> playerProfiles = new List<PlayerProfile>();
            foreach (Player player in players)
            {
                playerProfiles.Add(player.ToProfile());
            }
            return playerProfiles;
        }

        public static PlayerProfile ToProfile(this Player player)
        {
            return new PlayerProfile { 
                Id = player.Id,
                Name = player.Name,
                RegistrationDate = player.RegistrationDate,
                RegistrationTime = player.RegistrationTime,
                NormalStats = player.NormalStats,
                DailyStats = player.DailyStats
            };
        }

        public static PlayerProfile ToPlayerProfile(this Player player, int dailyGuesses)
        {
            return new PlayerProfile
            {
                Id = player.Id,
                Name = player.Name,
                RegistrationDate = player.RegistrationDate,
                RegistrationTime = player.RegistrationTime,
                NormalStats = player.NormalStats,
                DailyStats = player.DailyStats,
                DailyGuesses = dailyGuesses
            };
        }

        public static Player GetRandomPlayer()
        {
            return new Player
            {
                Name = StringExtensions.GetRandomNameLookingString(),
                RegistrationDate = DateTimeUtils.UtcNowDateString,
                RegistrationTime = DateTimeUtils.UtcNowTimeString,
                Email = StringExtensions.GetRandomNameLookingString()+"@hotmail.com",
                Password = StringExtensions.GetRandomNameLookingString(),
                EndedGames = GameExtensions.GetListOfRandomGames(),
                CurrentGame = GameExtensions.GetRandomGame(),
                DailyStats = GameStatisticExtensions.RandomGameStatistic(),
                NormalStats = GameStatisticExtensions.RandomGameStatistic()
            };
        }

    }
}
