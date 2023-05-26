namespace RotmgleWebApi.Games
{
    public struct CIEXYZ
    {
        /// <summary>
        /// Gets an empty CIEXYZ structure.
        /// </summary>
        public static readonly CIEXYZ Empty = new();

        /// <summary>
        /// Gets the CIE D65 (white) structure.
        /// </summary>
        public static readonly CIEXYZ D65 = new(0.9505, 1.0, 1.0890);

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public static bool operator ==(CIEXYZ item1, CIEXYZ item2)
        {
            return 
                item1.X == item2.X
                && item1.Y == item2.Y
                && item1.Z == item2.Z
                ;
        }

        public static bool operator !=(CIEXYZ item1, CIEXYZ item2)
        {
            return 
                item1.X != item2.X
                || item1.Y != item2.Y
                || item1.Z != item2.Z
                ;
        }

        public CIEXYZ(double x, double y, double z)
        {
            X = x > 0.9505 ? 0.9505 : x < 0 ? 0 : x;
            Y = y > 1.0 ? 1.0 : y < 0 ? 0 : y;
            Z = z > 1.089 ? 1.089 : z < 0 ? 0 : z;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;

            return this == (CIEXYZ)obj;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }
    }
}
