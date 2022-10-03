using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.Data.Extensions
{
    public static class RefreshTokenExtensions
    {
        public static bool IsExpired(this RefreshToken refreshToken)
        {
            return DateTime.UtcNow >= refreshToken.Expires;
        }

        public static bool IsRevoked(this RefreshToken refreshToken)
        {
            return refreshToken.Revoked.HasValue;
        }

        public static bool IsActive(this RefreshToken refreshToken)
        {
            return !refreshToken.IsExpired() && !refreshToken.IsRevoked();
        }

        public static void Revoke(this RefreshToken refreshToken)
        {
            refreshToken.Revoked = DateTime.UtcNow;
        }
    }
}
