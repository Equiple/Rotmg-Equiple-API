using RomgleWebApi.Data;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Models.Auth;
using RomgleWebApi.Services;
using RomgleWebApi.Utils;

namespace RomgleWebApi.Extensions
{
    public static class PlayerExtensions
    {
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

        public static bool HasActiveGame(this Player player)
        {
            return player.CurrentGame != null && !player.CurrentGame.IsEnded;
        }

        public static bool IsOutOfTries(this Player player)
        {
            return player.CurrentGame != null && player.CurrentGame.GuessItemIds.Count == 10;
        }

        public static Task<PlayerProfile> ToProfileAsync(
            this Player player,
            IItemsService itemsService,
            int dailyGuesses = 0)
        {
            return GetPlayerProfile(
                player,
                stats => stats.ToDetailed(itemsService),
                dailyGuesses);
        }

        public static Task<PlayerProfile> ToProfileAsync(
            this Player player,
            DetailedGameStatisticGetters getters = default,
            int dailyGuesses = 0)
        {
            return GetPlayerProfile(
                player,
                stats => stats.ToDetailed(getters: getters),
                dailyGuesses);
        }

        public static void RevokeRefreshTokens(this Player player)
        {
            foreach (RefreshToken token in player.RefreshTokens)
            {
                token.Revoke();
            }
        }

        public static void Randomize(this Player player)
        {
            player.Name = StringUtils.GenerateRandomNameLookingString();
            player.EndedGames = GameUtils.GenerateListOfRandomGames();
            player.CurrentGame = GameUtils.GenerateRandomGame();
            player.DailyStats.Randomize();
            player.NormalStats.Randomize();
        }

        private static async Task<PlayerProfile> GetPlayerProfile(
            Player player,
            Func<GameStatistic, Task<DetailedGameStatistic>> statsDetalization,
            int dailyGuesses)
        {
            return new PlayerProfile
            {
                Id = player.Id,
                Name = player.Name,
                RegistrationDate = player.RegistrationDate,
                NormalStats = await statsDetalization.Invoke(player.NormalStats),
                DailyStats = await statsDetalization.Invoke(player.DailyStats),
                DailyGuesses = dailyGuesses
            };
        }
    }
}
