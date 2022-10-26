namespace RomgleWebApi.Utils
{
    public static class CommonUtils
    {
        public static double MapValue(double x, double xLeft, double xRight, double resLeft, double resRight)
        {
            if (xLeft == xRight)
            {
                return resLeft;
            }
            return (x - xLeft) / (xRight - xLeft) * (resRight - resLeft) + resLeft;
        }
    }
}
