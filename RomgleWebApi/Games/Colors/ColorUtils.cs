using System.Drawing;

namespace RotmgleWebApi.Games
{
    public static class ColorUtils
    {
        public readonly static Color defaultGreen = ColorTranslator.FromHtml("#339900");

        public readonly static Color defaultRed = ColorTranslator.FromHtml("#cc0000");

        public static double GetDefaultGreenRedCIELabDistance() => defaultGreen.GetDistanceFrom(defaultRed);

        public readonly static List<Color> rainbowSmall = new()
        {
            Color.White, Color.Gray, Color.Black, Color.Brown, Color.Red, Color.Orange, Color.Yellow, Color.Green,
            Color.Blue, Color.Indigo, Color.Violet
        };

        public readonly static List<Color> differenceGradientRGBsmall = new()
        {
            ColorTranslator.FromHtml("#339900"),
            ColorTranslator.FromHtml("#666600"),
            ColorTranslator.FromHtml("#775500"),
            ColorTranslator.FromHtml("#884400"),
            ColorTranslator.FromHtml("#993300"),
            ColorTranslator.FromHtml("#cc0000")
        };

        //https://i.imgur.com/zQLHEkY.png
        public readonly static List<Color> differenceGradientRGB = new()
        {
            ColorTranslator.FromHtml("#339900"),
            ColorTranslator.FromHtml("#448800"),
            ColorTranslator.FromHtml("#557700"),
            ColorTranslator.FromHtml("#666600"),
            ColorTranslator.FromHtml("#775500"),
            ColorTranslator.FromHtml("#884400"),
            ColorTranslator.FromHtml("#993300"),
            ColorTranslator.FromHtml("#aa2200"),
            ColorTranslator.FromHtml("#bb1100"),
            ColorTranslator.FromHtml("#cc0000")
        };

        //https://i.imgur.com/oWLAVxm.png
        public readonly static List<Color> differenceGradientLAB = new()
        {
            ColorTranslator.FromHtml("#339900"),
            ColorTranslator.FromHtml("#569100"),
            ColorTranslator.FromHtml("#6d8800"),
            ColorTranslator.FromHtml("#7f7e00"),
            ColorTranslator.FromHtml("#8f7400"),
            ColorTranslator.FromHtml("#9d6800"),
            ColorTranslator.FromHtml("#aa5a00"),
            ColorTranslator.FromHtml("#b64a00"),
            ColorTranslator.FromHtml("#c13400"),
            ColorTranslator.FromHtml("#cc0000")
        };

        public static List<Color> ToShorterGradient(this List<Color> gradient)
        {
            List<Color> newGradient = new()
            {
                gradient[0],
                gradient[gradient.Count / 2 - gradient.Count / 2 / 2],
                gradient[gradient.Count / 2],
                gradient[gradient.Count - gradient.Count / 2 / 2],
                gradient[gradient.Count - 1],
            };
            return newGradient;
        }
    }
}
