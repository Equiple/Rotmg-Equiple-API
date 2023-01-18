using RomgleWebApi.Data.Models;
using RomgleWebApi.Utils;
using System.Drawing;
using System.Runtime.InteropServices;

namespace RomgleWebApi.Extensions
{
    public static class HintsExtensions
    {
        /// <summary>
        /// Determines whether or not there're enough guessed parameters 
        /// for final hint with Anagram and description
        /// </summary>
        /// <param name="hints"></param>
        /// <returns></returns>
        public static int CountCorrect(this Hints hints)
        {
            int count = 0;
            if (hints.Type == Hint.Correct)
            {
                count++;
            }
            if (hints.Tier == Hint.Correct)
            {
                count++;
            }
            if (hints.XpBonus == Hint.Correct)
            {
                count++;
            }
            if (hints.Feedpower == Hint.Correct)
            {
                count++;
            }
            if(ColorTranslator.FromHtml(hints.DominantColor) == ColorUtils.defaultGreen) 
            {
                count++;
            }
            return count;
        }

        public static int CountCorrect(this IReadOnlyList<Hints> hints)
        {
            int best = 0;
            foreach (Hints itemHints in hints)
            {
                int temp = itemHints.CountCorrect();
                if (temp > best)
                {
                    best = temp;
                }
            }
            return best;
        }
    }
}
