using System.Diagnostics.Metrics;

namespace RomgleWebApi.Utils
{
    public static class StringUtils
    {
        public static readonly List<String> DefaultNames = GetListOfDefaultNames();

        /// <summary>
        /// Gets a list of default names from a file.
        /// </summary>
        public static List<string> GetListOfDefaultNames()
        {
            List<string> names = new List<string>();
            foreach (string line in File.ReadLines(@"\Assets\DefaultNames.txt"))
            {
                names.Add(line);
            }
            return names;
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
            List<string> syllables = new List<string>();
            Random random = new Random();
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
            Random random = new Random();
            int length = random.Next(1, 8);
            List<string> randomList = new List<string>();
            for (int i = 0; i < length; i++)
            {
                randomList.Add(GenerateRandomNameLookingString());
            }
            return randomList;
        }
    }
}
