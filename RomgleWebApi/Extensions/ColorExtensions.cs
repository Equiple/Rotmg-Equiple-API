using RomgleWebApi.ColorConvertion;
using RomgleWebApi.Utils;
using System.Drawing;

namespace RomgleWebApi.Extensions
{
    public static class ColorExtensions
    {
        private const double _rgbValueLimit = 255.0;
        private const double _lModifier = 116.0;
        private const int _lConstant = 16;
        private const double _aModifier = 500.0;
        private const double _bModifier = 200.0;
        private const int _magicNumberThree = 3;
        private const double _delta = 6.0 / 29.0;
        private const double _rgbModifier = 0.055;
        private const double _rgbPow = 2.2;
        private const double _linearMargin = 0.04045;
        private const double _fMargin = 0.008856;
        private const double _pi2 = 2.0 * Math.PI;
        private const double _k = 1.0;
        private const double _aaa = 7.787;
        private const double _idk = 12.92;

        #region public methods

        /// <summary>
        /// Determines distance between two colors using CIEDE2000 Color-Difference Formula, 
        /// and the color that is \"radius\" away in direction \"angle\"."
        /// </summary>
        public static double GetPolarDistance(this CIELab color, CIELab secondColor, double radius, double angle)
        {
            var newA = color.A + radius * Math.Cos(angle);
            var newB = color.B + radius * Math.Sin(angle);
            CIELab newColor = CIELab.Empty;
            newColor.L = color.L;
            newColor.A = newA;
            newColor.B = newB;
            return newColor.DistanceFrom(secondColor);
        }

        /// <summary>
        /// Determines distance between two colors using their RGB values
        /// </summary>
        public static double GetRGBDistanceFrom(this Color color, Color secondColor)
        {
            double red = Math.Pow(Convert.ToDouble(color.R) - secondColor.R, 2.0);
            double green = Math.Pow(Convert.ToDouble(color.G) - secondColor.G, 2.0);
            double blue = Math.Pow(Convert.ToDouble(color.B) - secondColor.B, 2.0);
            double distance = Math.Sqrt(blue + green + red);
            return distance;
        }

        /// <summary>
        /// Determines distance between two colors using CIEDE2000 Color-Difference Formula.
        /// </summary>
        public static double GetDistanceFrom(this Color color, Color secondColor)
        {
            CIELab colorTwo = secondColor.ToXYZ().ToLab();
            return color.ToXYZ().ToLab().DistanceFrom(colorTwo);
        }

        /// <summary>
        /// Determines distance between two colors using CIEDE2000 Color-Difference Formula.
        /// </summary>
        public static double DistanceFrom(this CIELab color, CIELab secondColor)
        {
            double mCs = (Math.Sqrt(color.A * color.A + color.B * color.B) + 
                Math.Sqrt(secondColor.A * secondColor.A + secondColor.B * secondColor.B)) / 2.0;
            double G = 0.5 * (1.0 - Math.Sqrt(Math.Pow(mCs, 7) / (Math.Pow(mCs, 7) + Math.Pow(25.0, 7))));
            double a1p = (_k + G) * color.A;
            double a2p = (_k + G) * secondColor.A;
            double C1p = Math.Sqrt(a1p * a1p + color.B * color.B);
            double C2p = Math.Sqrt(a2p * a2p + secondColor.B * secondColor.B);
            double h1p = Math.Abs(a1p) + Math.Abs(color.B) > double.Epsilon ? Math.Atan2(color.B, a1p) : 0.0;
            if (h1p < 0.0) 
            {
                h1p += _pi2;
            }
            double h2p = Math.Abs(a2p) + Math.Abs(secondColor.B) > double.Epsilon ? Math.Atan2(secondColor.B, a2p) : 0.0;
            if (h2p < 0.0) 
            { 
                h2p += _pi2;
            }
            double dLp = secondColor.L - color.L;
            double dCp = C2p - C1p;
            double _zero = 0.0;
            double cProdAbs = Math.Abs(C1p * C2p);
            if (cProdAbs > double.Epsilon && Math.Abs(h1p - h2p) <= Math.PI)
            {
                _zero = h2p - h1p;
            }
            else if (cProdAbs > double.Epsilon && h2p - h1p > Math.PI)
            {
                _zero = h2p - h1p - _pi2;
            }
            else if (cProdAbs > Double.Epsilon && h2p - h1p < -Math.PI)
            {
                _zero = h2p - h1p + _pi2;
            }
            double dHp = 2.0 * Math.Sqrt(C1p * C2p) * Math.Sin(_zero / 2.0);
            double mLp = (color.L + secondColor.L) / 2.0;
            double mCp = (C1p + C2p) / 2.0;
            double mhp = 0.0;
            if (cProdAbs > double.Epsilon && Math.Abs(h1p - h2p) <= Math.PI)
            {
                mhp = (h1p + h2p) / 2.0;
            }
            else if (cProdAbs > double.Epsilon && Math.Abs(h1p - h2p) > Math.PI && h1p + h2p < _pi2)
            {
                mhp = (h1p + h2p + _pi2) / 2.0;
            }
            else if (cProdAbs > double.Epsilon && Math.Abs(h1p - h2p) > Math.PI && h1p + h2p >= _pi2)
            {
                mhp = (h1p + h2p - _pi2) / 2.0;
            }
            else if (cProdAbs <= double.Epsilon)
            {
                mhp = h1p + h2p;
            }
            double T = 1.0 - 0.17 * Math.Cos(mhp - Math.PI / 6.0) + .24 * Math.Cos(2.0 * mhp) +
                0.32 * Math.Cos(3.0 * mhp + Math.PI / 30.0) - 0.2 * Math.Cos(4.0 * mhp - 7.0 * Math.PI / 20.0);
            double dTheta = Math.PI / 6.0 * Math.Exp(-Math.Pow((mhp / (2.0 * Math.PI) * 360.0 - 275.0) / 25.0, 2));
            double RC = 2.0 * Math.Sqrt(Math.Pow(mCp, 7) / (Math.Pow(mCp, 7) + Math.Pow(25.0, 7)));
            double mlpSqr = (mLp - 50.0) * (mLp - 50.0);
            double SL = _k + 0.015 * mlpSqr / Math.Sqrt(20.0 + mlpSqr);
            double SC = _k + 0.045 * mCp;
            double SH = _k + 0.015 * mCp * T;
            double RT = -Math.Sin(2.0 * dTheta) * RC;
            double ciede2000 = Math.Sqrt(Math.Pow(dLp / (_k * SL), 2) + Math.Pow(dCp / (_k * SC), 2) + Math.Pow(dHp / (_k * SH), 2) +
                RT * dCp / (_k * SC) * dHp / (_k * SH));
            return ciede2000;
        }

        /// <summary>
        /// Converts RGB Color to CIEXYZ.
        /// </summary>
        public static CIEXYZ ToXYZ(this Color color)
        {
            // normalize red, green, blue values
            double redLinear = (double)color.R / _rgbValueLimit;
            double greenLinear = (double)color.G / _rgbValueLimit;
            double blueLinear = (double)color.B / _rgbValueLimit;

            // convert to a sRGB form
            double r = (redLinear > _linearMargin) ? Math.Pow((redLinear + _rgbModifier) / 
                (1 + _rgbModifier), _rgbPow) : (redLinear / _idk);
            double g = (greenLinear > _linearMargin) ? Math.Pow((greenLinear + _rgbModifier) /
                (1 + _rgbModifier), _rgbPow) : (greenLinear / _idk);
            double b = (blueLinear > _linearMargin) ? Math.Pow((blueLinear + _rgbModifier) / 
                (1 + _rgbModifier), _rgbPow) : (blueLinear / _idk);

            double x = (r * 0.4124 + g * 0.3576 + b * 0.1805);
            double y = (r * 0.2126 + g * 0.7152 + b * 0.0722);
            double z = (r * 0.0193 + g * 0.1192 + b * 0.9505);

            // converts
            return new CIEXYZ(x, y, z);
        }

        /// <summary>
        /// Converts CIELab to CIEXYZ.
        /// </summary>
        public static CIEXYZ ToXYZ(this CIELab color)
        {
            double fy = (color.L + _lConstant) / _lModifier;
            double fx = fy + (color.A / _aModifier);
            double fz = fy - (color.B / _bModifier);

            var x = (fx > _delta) 
                ? CIEXYZ.D65.X * (fx * fx * fx) 
                : (fx - _lConstant / _lModifier) * _magicNumberThree * (_delta * _delta) * CIEXYZ.D65.X;

            var y = (fy > _delta) 
                ? CIEXYZ.D65.Y * (fy * fy * fy) 
                : (fy - _lConstant / _lModifier) * _magicNumberThree * (_delta * _delta) * CIEXYZ.D65.Y;

            var z = (fz > _delta) 
                ? CIEXYZ.D65.Z * (fz * fz * fz) 
                : (fz - _lConstant / _lModifier) * _magicNumberThree * (_delta * _delta) * CIEXYZ.D65.Z;

            return new CIEXYZ(x, y, x);
        }

        /// <summary>   
        /// Converts RGB Color to CIELab.
        /// </summary>
        public static CIELab ToLab(this Color color)
        {
            return ToLab(color.ToXYZ());
        }

        /// <summary>
        /// Converts CIEXYZ Color to CIELab.
        /// </summary>
        public static CIELab ToLab(this CIEXYZ color)
        {
            CIELab lab = CIELab.Empty;
            lab.L = _lModifier * FXYZ(color.Y / CIEXYZ.D65.Y) - _lConstant;
            lab.A = _aModifier * (FXYZ(color.X / CIEXYZ.D65.X) - FXYZ(color.Y / CIEXYZ.D65.Y));
            lab.B = _bModifier * (FXYZ(color.Z / CIEXYZ.D65.Y) - FXYZ(color.Z / CIEXYZ.D65.Z));
            return lab;
        }

        /// <summary>
        /// Converts CIELab to RGB.
        /// </summary>
        public static Color ToColor(this CIELab color)
        {
            return ToColor(ToXYZ(color));
        }

        /// <summary>
        /// Converts CIEXYZ to RGB structure.
        /// </summary>
        public static Color ToColor(this CIEXYZ color)
        {
            double[] cLinear = new double[3];
            cLinear[0] = color.X * 3.2410 - color.Y * 1.5374 - color.Z * 0.4986;
            cLinear[1] = -color.X * 0.9692 + color.Y * 1.8760 - color.Z * 0.0416;
            cLinear[2] = color.X * 0.0556 - color.Y * 0.2040 + color.Z * 1.0570;

            for (int i = 0; i < 3; i++)
            {
                cLinear[i] = (cLinear[i] <= 0.0031308)
                    ? 12.92 * cLinear[i]
                    : Math.Pow((cLinear[i] + _rgbModifier) / 1.055, 2.4);
            }

            int r = (int)Math.Abs(cLinear[0] * _rgbValueLimit);
            int g = (int)Math.Abs(cLinear[1] * _rgbValueLimit);
            int b = (int)Math.Abs(cLinear[2] * _rgbValueLimit);

            return Color.FromArgb(r, g, b);
        }

        #endregion public methods

        #region private methods

        /// <summary>
        /// XYZ to L*a*b* transformation function.
        /// </summary>
        private static double FXYZ(double t)
        {
            return ((t > _fMargin) 
                ? Math.Pow(t, (1.0 / 3.0)) 
                : (_aaa * t + 16.0 / _lModifier));
        }

        #endregion private methods
    }
}
