using MongoDB.Driver;
using RomgleWebApi.Data.Models;

namespace RomgleWebApi.Data.Extensions
{
    public static class ItemExtension
    {
        public static string ToString(this Item item)
        {
            return $"#{item.Type}: {item.Name} [{item.Tier}{item.IsReskin()}]\nDamage: {item.GetDamageRange()}\nRange: {item.Range} tiles\nShots: " +
                $"{item.NumberOfShots}\nXP bonus: {item.XpBonus}%\nFeed power: {item.Feedpower}\nColor:{item.DominantColor} #{item.ColorClass} " +
                $"\n<{item.Tags}>\n";
        }

        private static string IsReskin(this Item item)
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
    }
}
