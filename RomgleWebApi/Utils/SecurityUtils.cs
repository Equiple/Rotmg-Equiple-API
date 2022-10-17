using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

namespace RomgleWebApi.Utils
{
    public static class SecurityUtils
    {
        public static string GenerateBase64SecurityKey(int byteCount = 64)
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(byteCount));
        }

        public static SecurityKey GetSecurityKey(string key)
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));
        }
    }
}
