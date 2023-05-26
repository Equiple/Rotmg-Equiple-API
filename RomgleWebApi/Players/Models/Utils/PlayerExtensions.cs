using RotmgleWebApi.Games;
using RotmgleWebApi.Items;

namespace RotmgleWebApi.Players
{
    public static class PlayerExtensions
    {
        /// <summary>
        /// Returns GameStatistic for given player of specified gamemode.
        /// </summary>
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

        /// <summary>
        /// Returns true if the player run out of tries, false otherwise.
        /// </summary>
        public static bool IsOutOfTries(this Player player)
        {
            return player.CurrentGame != null && player.CurrentGame.GuessItemIds.Count == 10;
        }

        /// <summary>
        /// Converts Player to a PlayerProfile.
        /// </summary>
        public static async Task<PlayerProfile> ToProfileAsync(
            this Player player,
            IItemService itemService,
            int dailyGuesses = 0)
        {
            PlayerProfile playerProfile = new()
            {
                Id = player.Id,
                Name = player.Name,
                Role = player.Role,
                RegistrationDate = player.RegistrationDate,
                NormalStats = await player.NormalStats.ToDetailed(itemService),
                DailyStats = await player.DailyStats.ToDetailed(itemService),
                DailyGuesses = dailyGuesses,
            };
            return playerProfile;
        }

        /// <summary>
        /// Randomizes fields of a Player.
        /// </summary>
        public static void Randomize(this Player player)
        {
            player.Name = StringUtils.GenerateRandomNameLookingString();
            player.Role = "user";
            player.EndedGames = GameUtils.GenerateListOfRandomGames();
            player.CurrentGame = GameUtils.GenerateRandomGame();
            player.DailyStats.Randomize();
            player.NormalStats.Randomize();
        }
    }
}
