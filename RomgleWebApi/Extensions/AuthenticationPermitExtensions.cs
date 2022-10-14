using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.Extensions
{
    public static class AuthenticationPermitExtensions
    {
        public static Identity CreateIdentity(
            this AuthenticationPermit permit,
            string id,
            IdentityDetails details)
        {
            return new Identity
            {
                Provider = permit.Provider,
                Id = id,
                Details = details
            };
        }
    }
}
