using RomgleWebApi.Extensions;
using System.Drawing;

namespace RomgleWebApi.Utils
{
    public static class ColorUtils
    {
        public readonly static Color defaultGreen = Color
            .FromArgb(alpha: 255, red: 51, green: 153, blue: 0);
        public readonly static Color defaultRed = Color
            .FromArgb(alpha: 255, red: 204, green: 0, blue: 0);

        public static double GetDefaultGreenRedCIELabDistance() => defaultGreen.GetDistanceFrom(defaultRed);
    }
}
