using RomgleWebApi.Data;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Models.Auth;
using RomgleWebApi.Services;
using RomgleWebApi.Utils;

namespace RomgleWebApi.Extensions
{
    public static class PlayerExtensions
    {
        #region public methods 

        /// <summary>
        /// Gets device of a player by Id
        /// </summary>
        public static Device GetDevice(this Player player, string deviceId)
        {
            Device device = player.Devices.First(device => device.Id == deviceId);
            return device;
        }

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
        /// Returns true if the player has an active game, false otherwise.
        /// </summary>
        public static bool HasActiveGame(this Player player)
        {
            return player.CurrentGame != null && !player.CurrentGame.IsEnded;
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
        public static Task<PlayerProfile> ToProfileAsync(
            this Player player,
            IItemService itemsService,
            int dailyGuesses = 0)
        {
            return GetPlayerProfile(
                player,
                stats => stats.ToDetailed(itemsService),
                dailyGuesses);
        }

        /// <summary>
        /// Converts Player to a PlayerProfile.
        /// </summary>
        public static Task<PlayerProfile> ToProfileAsync(
            this Player player,
            DetailedGameStatisticGetters getters = default,
            int dailyGuesses = 0)
        {
            return GetPlayerProfile(
                player,
                stats => stats.ToDetailedAsync(getters: getters),
                dailyGuesses);
        }

        /// <summary>
        /// Randomizes fields of a Player.
        /// </summary>
        public static void Randomize(this Player player)
        {
            player.Name = StringUtils.GenerateRandomNameLookingString();
            player.EndedGames = GameUtils.GenerateListOfRandomGames();
            player.CurrentGame = GameUtils.GenerateRandomGame();
            player.DailyStats.Randomize();
            player.NormalStats.Randomize();
        }

        #endregion public methods

        #region private methods

        /// <summary>
        /// Returns a PlayerProfile for a Player.
        /// </summary>
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

        #endregion private methods
    }
}
