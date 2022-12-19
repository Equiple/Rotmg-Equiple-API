using System.Text.RegularExpressions;

namespace RomgleWebApi.Extensions
{
    public static class StringExtensions
    {
        private static Random random = new Random();

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
        /// A double value if parsing was successful, null if not.
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
        /// An integer value if parsing was successful, null if not.
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
        /// Replaces unicode and html tags with their normal text counterparts or removes them altogether.
        /// </summary>
        /// <param name="givenString"></param>
        /// <returns>
        /// A string with found values replaced.
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
        /// 
        /// </summary>
        /// <param name="givenString"></param>
        /// <returns></returns>
        public static string RemoveSymbols(this string givenString, string symbols)
        {
            string str = "";
            foreach(char letter in symbols)
            {
                str = givenString.Replace(letter + "", "");
            }
            return str;
        }

        /// <summary>
        /// Shuffles string
        /// </summary>
        public static string Shuffle(this string str)
        {
            char[] array = str.ToCharArray();
            int counter = array.Length;
            while (counter > 1)
            {
                counter--;
                int k = random.Next(counter + 1);
                var value = array[k];
                array[k] = array[counter];
                array[counter] = value;
            }
            return new string(array);
        }

        public static bool Contains(this string str, List<string> list)
        {
            foreach (string line in list)
            {
                if (str.Contains(line))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
