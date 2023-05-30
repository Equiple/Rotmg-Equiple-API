namespace RotmgleWebApi
{
    public static class StringUtils
    {
        public static readonly List<string> DefaultNames =
            GetListFromFile(@"Assets\DefaultNames.txt");

        public static readonly List<string> IgnoredWords =
            GetListFromFile(@"Assets\IgnoredWords.txt");

        /// <summary>
        /// Gets a list from specified text document.
        /// </summary>
        public static List<string> GetListFromFile(string path)
        {
            if (!path.Contains(".txt"))
            {
                throw new Exception("Given file is not a text document.");
            }
            List<string> list = new();
            foreach (string line in File.ReadLines(path))
            {
                list.Add(line);
            }
            return list;
        }

        /// <summary>
        /// Generates a string that looks like a name.
        /// </summary>
        /// <returns>
        /// String of random length that consists of random syllables.
        /// </returns>
        public static string GenerateRandomNameLookingString()
        {
            string result = "";
            char[] consolants = "qwrtpsdfghjklzxcvbnm".ToCharArray();
            char[] vowels = "eyuioa".ToCharArray();
            List<string> syllables = new();
            Random random = new();
            foreach (char cons in consolants)
            {
                foreach (char vow in vowels)
                {
                    syllables.Add("" + cons + vow);
                }
            }
            int length = random.Next(2, 10);
            for (int i = 0; i < length; i++)
            {
                int index = random.Next(0, syllables.Count);
                result += syllables[index];
            }
            return result;
        }

        /// <summary>
        /// Generates a list of random strings with random length.
        /// </summary>
        /// <returns>
        /// List of random strings. 
        /// </returns>
        public static List<string> GenerateRandomListOfStrings()
        {
            Random random = new();
            int length = random.Next(1, 8);
            List<string> randomList = new();
            for (int i = 0; i < length; i++)
            {
                randomList.Add(GenerateRandomNameLookingString());
            }
            return randomList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetRandomDefaultName()
        {
            Random random = new();
            return DefaultNames[random.Next(DefaultNames.Count)];
        }
    }
}
