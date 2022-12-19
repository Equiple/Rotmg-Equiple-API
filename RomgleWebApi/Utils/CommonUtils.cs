﻿namespace RomgleWebApi.Utils
{
    public static class CommonUtils
    {
        public static double MapValue(double value, double fromLow, double fromHigh, double toLow, double toHigh)
        {
            if (fromLow == fromHigh)
            {
                return toLow;
            }
            return (value - fromLow) / (fromHigh - fromLow) * (toHigh - toLow) + toLow;
        }
    }
}
