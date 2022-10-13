using RomgleWebApi.Data.Models;
using RomgleWebApi.Utils;

namespace RomgleWebApi.Data.Extensions
{
    public static class GameExtensions
    {
        /// <summary>
        /// Creates a random Game
        /// </summary>
        /// <returns>Random Game</returns>
        public static Game GetRandomGame()
        {
            Random random = new Random();
            return new Game
            {
                Mode = Gamemode.Normal,
                TargetItemId = "633708e137780b036541114a",
                GuessItemIds = StringExtensions.GetRandomListOfStrings(),
                StartDate = DateTimeUtils.UtcNowDateString,
                StartTime = DateTimeUtils.UtcNowTimeString,
                ReskingExcluded = false,
                IsEnded = true,
                GameResult = (GameResult)random.Next(0, 2)
            };
        }

        /// <summary>
        /// Creates a list of randomly generated list of Games
        /// </summary>
        /// <returns>
        /// List of games with random length
        /// </returns>
        public static List<Game> GetListOfRandomGames()
        {
            Random random = new Random();
            List<Game> listOfGames = new List<Game>();
            int length = random.Next(1, 8);
            for(int i = 0; i < length; i++)
            {
                listOfGames.Add(GetRandomGame());
            }
            listOfGames[listOfGames.Count - 1].Mode = Gamemode.Daily;
            listOfGames[listOfGames.Count - 1].GameResult = GameResult.Won;
            return listOfGames;
        }
    }
}
