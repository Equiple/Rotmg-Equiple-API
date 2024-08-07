﻿namespace RotmgleWebApi.Games
{
    public static class GameUtils
    {
        /// <summary>
        /// Creates a random Game
        /// </summary>
        /// <returns>Random Game</returns>
        public static Game GenerateRandomGame()
        {
            Random random = new();
            return new Game
            {
                Mode = Gamemode.Normal,
                TargetItemId = "633708e137780b036541114a",
                GuessItemIds = StringUtils.GenerateRandomListOfStrings(),
                StartDate = DateTime.UtcNow,
                ReskingExcluded = false,
                IsEnded = true,
                GameResult = (GameResult)random.Next(0, 2),
            };
        }

        /// <summary>
        /// Creates a list of randomly generated list of Games
        /// </summary>
        /// <returns>
        /// List of games with random length
        /// </returns>
        public static List<Game> GenerateListOfRandomGames()
        {
            Random random = new();
            List<Game> listOfGames = new();
            int length = random.Next(1, 8);
            for (int i = 0; i < length; i++)
            {
                listOfGames.Add(GenerateRandomGame());
            }
            listOfGames[^1].Mode = Gamemode.Daily;
            listOfGames[^1].GameResult = GameResult.Won;
            return listOfGames;
        }
    }
}
