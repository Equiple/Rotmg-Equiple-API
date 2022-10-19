using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.Extensions
{
    public static class RefreshTokenExtensions
    {
        public static bool IsExpired(this RefreshToken refreshToken)
        {
            return DateTime.UtcNow >= refreshToken.Expires;
        }

        public static bool IsRevoked(this RefreshToken refreshToken)
        {
            return refreshToken.Revoked != null;
        }

        public static bool IsActive(this RefreshToken refreshToken)
        {
            return !refreshToken.IsExpired() && !refreshToken.IsRevoked();
        }

        public static void Revoke(this RefreshToken refreshToken)
        {
            if (!refreshToken.IsActive())
            {
                return;
            }
            refreshToken.Revoked = DateTime.UtcNow;
        }
    }
}
