using System.Text.RegularExpressions;

namespace RomgleWebApi.Data.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Removes everything from the string starting from the first space.
        /// </summary>
        /// <param name="givenString"></param>
        /// <returns>
        /// Substring of given string before the first space.
        /// </returns>
        public static string Strip(this string givenString)
        {
            int removalIndex = givenString.IndexOf(" ");
            if (removalIndex > 0)
            {
                return givenString.Remove(removalIndex);
            }
            return givenString;
        }

        /// <summary>
        /// Tries to parse double from the string, if not possible returns null.
        /// </summary>
        /// <param name="givenString"></param>
        /// <returns>
        /// double value if parsing was successful, null if not.
        /// </returns>
        public static double? ParseDouble(this string givenString)
        {
            double result = 0;
            if (double.TryParse(givenString, out result))
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// Tries to parse int from the string, if not possible returns null.
        /// </summary>
        /// <param name="givenString"></param>
        /// <returns>
        /// int value if parsing was successful, null if not.
        /// </returns>
        public static int? ParseInt(this string givenString)
        {
            int result = 0;
            if (int.TryParse(givenString, out result))
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// Trying to parse int from the string, if not possible returns null.
        /// </summary>
        /// <param name="givenString"></param>
        /// <returns>
        /// int value if parsing was successful, null if not.
        /// </returns>
        public static List<int?> ParseInt(this string[] givenString)
        {
            List<int?> intArray = new List<int?>();
            foreach (string line in givenString)
            {
                intArray.Add(line.ParseInt());
            }
            return intArray;
        }

        /// <summary>
        /// Removes unicode html tags, such as '&nbsp;' and '&apos;'.
        /// </summary>
        /// <param name="givenString">Initial String.</param>
        /// <param name="replaceWith">String to replace substrings with.</param>
        /// <returns>
        /// Strings with found values replaced.
        /// </returns>
        public static string ReplaceGarbage(this string givenString, string replaceWith)
        {
            givenString.Replace("&nbsp;", replaceWith);
            givenString.Replace("&apos;", replaceWith);
            return givenString;
        }

        /// <summary>
        /// Removes unicode html tags, such as '&nbsp;' and '&apos;' with their normal text counterparts.
        /// </summary>
        /// <param name="givenString">Initial String.</param>
        /// <returns>
        /// Strings with found values replaced.
        /// </returns>
        public static string ReplaceGarbage(this string givenString)
        {
            givenString.Replace("&nbsp;", " ");
            givenString.Replace("&apos;", "\'");
            return givenString;
        }

        /// <summary>
        /// Replaces unicode and html tags with their normal text counterparts or removes them altogether.
        /// </summary>
        /// <param name="givenString"></param>
        /// <returns>
        /// String with found values replaced.
        /// </returns>
        public static string ScrubHtml(this string givenString)
        {
            string step1 = Regex.Replace(givenString, @"<[^>]+>", "").Trim();
            string step2 = Regex.Replace(step1, @"&nbsp;", " ");
            string step3 = Regex.Replace(step2, @"\s{2,}", " ");
            string step4 = Regex.Replace(step3, @"&apos;", "\'");
            string step5 = Regex.Replace(step4, @",", "");
            string step6 = Regex.Replace(step5, @"'", "\'");
            return step6;
        }

        /// <summary>
        /// Changes word form from plural to singular.
        /// </summary>
        /// <param name="word">Initial word.</param>
        /// <returns>
        /// Word in singular form if replacement was successful, initial word otherwise.
        /// </returns>
        public static string ToSingular(this string word)
        {
            word = word.Replace("ies", "y");
            word = word.Replace("s", "");
            return word;
        }

        /// <summary>
        /// Generates a string that looks like a name.
        /// </summary>
        /// <returns>
        /// String of random length that consists of random syllables.
        /// </returns>
        public static string GetRandomNameLookingString()
        {
            string result = "";
            char[] consolants = "qwrtpsdfghjklzxcvbnm".ToCharArray();
            char[] vowels = "eyuioa".ToCharArray();
            List<string> syllables = new List<string>();
            Random random = new Random();
            foreach(char cons in consolants)
            {
                foreach(char vow in vowels)
                {
                    syllables.Add("" + cons + vow);
                }
            }
            int length = random.Next(2, 10);
            for(int i = 0; i < length; i++)
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
        public static List<string> GetRandomListOfStrings()
        {
            Random random = new Random();
            int length = random.Next(1, 8);
            List<string> randomList = new List<string>();
            for(int i = 0; i < length; i++)
            {
                randomList.Add(GetRandomNameLookingString());
            }
            return randomList;
        }
    }
}
