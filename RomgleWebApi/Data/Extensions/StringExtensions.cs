using System.Text.RegularExpressions;

namespace RomgleWebApi.Data.Extensions
{
    public static class StringExtensions
    {
        public static string Strip(this string str)
        {
            int removalIndex = str.IndexOf(" ");
            if (removalIndex > 0)
            {
                return str.Remove(removalIndex);
            }
            return str;
        }

        public static double? ParseDouble(this string str)
        {
            double result = 0;
            if (double.TryParse(str, out result))
            {
                return result;
            }
            return null;
        }
        public static int? ParseInt(this string str)
        {
            int result = 0;
            if (int.TryParse(str, out result))
            {
                return result;
            }
            return null;
        }

        public static List<int?> ParseInt(this string[] str)
        {
            List<int?> intArray = new List<int?>();
            foreach (string line in str)
            {
                intArray.Add(line.ParseInt());
            }
            return intArray;
        }

        public static string ReplaceGarbage(this string str, string replaceWith)
        {
            str.Replace("&nbsp;", replaceWith);
            str.Replace("&apos;", replaceWith);
            return str;
        }

        public static string ReplaceGarbage(this string str)
        {
            str.Replace("&nbsp;", " ");
            str.Replace("&apos;", "\'");
            return str;
        }
        public static string ScrubHtml(this string str)
        {
            string step1 = Regex.Replace(str, @"<[^>]+>", "").Trim();
            string step2 = Regex.Replace(step1, @"&nbsp;", " ");
            string step3 = Regex.Replace(step2, @"\s{2,}", " ");
            string step4 = Regex.Replace(step3, @"&apos;", "\'");
            string step5 = Regex.Replace(step4, @",", "");
            string step6 = Regex.Replace(step5, @"'", "\'");
            return step6;
        }

        public static string ToSingular(this string str)
        {
            str = str.Replace("ies", "y");
            str = str.Replace("s", "");
            return str;
        }


    }
}
