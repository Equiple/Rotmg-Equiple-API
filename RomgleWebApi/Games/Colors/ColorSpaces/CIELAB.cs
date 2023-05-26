namespace RotmgleWebApi.Games
{
    public struct CIELab
    {
        /// <summary>
        /// Gets an empty CIELab structure.
        /// </summary>
        public static readonly CIELab Empty = new();

        public double L { get; set; }
        public double A { get; set; }
        public double B { get; set; }


        public static bool operator ==(CIELab item1, CIELab item2)
        {
            return 
                item1.L == item2.L
                && item1.A == item2.A
                && item1.B == item2.B
                ;
        }

        public static bool operator !=(CIELab item1, CIELab item2)
        {
            return 
                item1.L != item2.L
                || item1.A != item2.A
                || item1.B != item2.B
                ;
        }

        public CIELab(double l, double a, double b)
        {
            L = l;
            A = a;
            B = b;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;

            return this == (CIELab)obj;
        }

        public override int GetHashCode()
        {
            return L.GetHashCode() ^ A.GetHashCode() ^ B.GetHashCode();
        }
    }
}
