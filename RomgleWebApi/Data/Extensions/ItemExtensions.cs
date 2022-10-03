using MongoDB.Driver;
using RomgleWebApi.Data.Models;

namespace RomgleWebApi.Data.Extensions
{
    public static class ItemExtensions
    {
        public static string ToString(this Item item)
        {
            return $"#{item.Type}: {item.Name} [{item.Tier}{item.GetReskinText()}]\nDamage: {item.GetDamageRange()}\nRange: {item.Range} tiles\nShots: " +
                $"{item.NumberOfShots}\nXP bonus: {item.XpBonus}%\nFeed power: {item.Feedpower}\nColor:{item.DominantColor} #{item.ColorClass} " +
                $"\n<{item.Tags}>\n";
        }

        public static string ConvertToString(this Item item)
        {
            return $"#{item.Type}: {item.Name} [{item.Tier} {item.GetReskinText()}] {item.XmlId}\nDamage: {item.GetDamageRange()}\nRange: {item.Range} tiles\nShots: " +
                $"{item.NumberOfShots}\nXP bonus: {item.XpBonus}%\nFeed power: {item.Feedpower}\nColor:{item.DominantColor} #{item.ColorClass} " +
                $"\n<{item.GetTags()}>\n";
        }

        private static string GetTags(this Item item)
        {
            string tags = "";
            foreach (string tag in item.Tags)
            {
                tags += tag + " ";
            }
            return tags;
        }

        private static string GetReskinText(this Item item)
        {
            if (!item.Reskin)
            {
                return "";
            }
            else return " Reskin";
        }

        public static void DamageRangeToMargins(this Item item, string str)
        {
            List<int?> damageMargins = new List<int?>();
            damageMargins = str.Replace('–', ' ').Split(' ').ParseInt();
            item.DamageBottom = damageMargins.First() ?? 0;
            if (damageMargins.Count > 1)
            {
                item.DamageTop = damageMargins.ElementAt(1);
            }
        }

        private static string GetDamageRange(this Item item)
        {
            string str = $"{item.DamageBottom}";
            if (item.DamageTop != null)
            {
                str += $"-{item.DamageTop}";
            }
            return str;
        }

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
    }
}
