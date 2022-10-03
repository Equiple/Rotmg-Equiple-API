using System.IdentityModel.Tokens.Jwt;

namespace RomgleWebApi.Utils
{
    public static class ClaimNames
    {
        public const string UserId = JwtRegisteredClaimNames.Sub;
    }
}
