using MongoDB.Driver;

namespace RotmgleWebApi.Items
{
    public static class ItemExtensions
    {
        #region public methods

        /// <summary>
        /// Returns Item object in a readable form in string.
        /// </summary>
        public static string ToString(this Item item)
        {
            return $"#{item.Type}: {item.Name} [{item.Tier}{item.GetReskinText()}]\nDamage: {item.GetDamageRange()}\nRange: {item.Range} tiles\nShots: " +
                $"{item.NumberOfShots}\nXP bonus: {item.XpBonus}%\nFeed power: {item.Feedpower}\nColor:{item.DominantColor} #{item.ColorClass} " +
                $"\n<{item.Tags}>\n";
        }

        /// <summary>
        /// Returns Item object in a readable form in string.
        /// </summary>
        public static string ConvertToString(this Item item)
        {
            return $"#{item.Type}: {item.Name} [{item.Tier} {item.GetReskinText()}] {item.XmlId}\nDamage: {item.GetDamageRange()}\nRange: {item.Range} tiles\nShots: " +
                $"{item.NumberOfShots}\nXP bonus: {item.XpBonus}%\nFeed power: {item.Feedpower}\nColor:{item.DominantColor} #{item.ColorClass} " +
                $"\n<{item.GetTags()}>\n";
        }

        /// <summary>
        /// Returns tags of an Item in a string separated by a space.
        /// </summary>
        private static string GetTags(this Item item)
        {
            string tags = "";
            foreach (string tag in item.Tags)
            {
                tags += tag + " ";
            }
            return tags;
        }

        /// <summary>
        /// Attempts to parse int from a string and set Damage values of Item to result.
        /// </summary>
        public static void DamageRangeToMargins(this Item item, string str)
        {
            List<int?> damageMargins = str.Replace('–', ' ').Split(' ').ParseInt();
            item.DamageBottom = damageMargins[0] ?? 0;
            if (damageMargins.Count > 1)
            {
                item.DamageTop = damageMargins[1];
            }
        }

        /// <summary>
        /// Attempts creating a string with Tags separated with spaces, based off Item fields.
        /// </summary>
        public static string GenerateLabels(this Item item)
        {
            string labels = "";
            labels += $" {item.Tier} {item.Type} {item.GetReskinText()} {item.DominantColor} {item.ColorClass} ";
            if (item.Type == "Weapon")
            {
                labels += "basetype ";
            }
            if (item.Tier == "UT" || item.Tier == "ST")
            {
                labels += "soulbound ";
            }
            else
            {
                labels += "tradeable ";
            }
            return labels.ToLower();
        }

        /// <summary>
        /// 
        /// </summary>
        public static string GenerateAnagramIfEligible(this Item item, int hintsCount)
        {
            if (hintsCount < 3)
            {
                return "???";
            }
            string name = item.Name
                .RemoveSymbols(",.'`[]{}()-*_:;\"!?")
                .ToLower();
            List<string> initialWords = name.Split(' ').ToList();
            string anagram = "";
            foreach (char letter in name)
            {
                if (!anagram.Contains(letter) && letter != ' ')
                {
                    anagram += letter;
                }
            }
            bool check = true;
            while (check)
            {
                anagram = anagram.Shuffle();
                if (!anagram.Contains(StringUtils.IgnoredWords) && !anagram.Contains(initialWords))
                {
                    check = false;
                }
            }
            return anagram;
        }

        public static string GetDescriptionIfEligible(this Item item, int hintsCount)
        {
            if (hintsCount < 4)
            {
                return "???";
            }
            else return item.Description;
        }

        #endregion public methods

        #region private methods

        /// <summary>
        /// Converts boolean field Reskin to a string, mostly needed for ToString() methods.
        /// </summary>
        private static string GetReskinText(this Item item)
        {
            if (!item.Reskin)
            {
                return "";
            }
            else return " Reskin";
        }

        /// <summary>
        /// Converts damage values of an Item to DamageBottom-DamageTop 
        /// </summary>
        private static string GetDamageRange(this Item item)
        {
            string str = $"{item.DamageBottom}";
            if (item.DamageTop != null)
            {
                str += $"-{item.DamageTop}";
            }
            return str;
        }

        #endregion private methods
    }
}
