using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.Data.Extensions
{
    public static class AuthenticationPermitExtensions
    {
        public static Identity CreateIdentity(this AuthenticationPermit permit, string id)
        {
            return new Identity
            {
                Provider = permit.Provider,
                Id = id,
                Details = permit.Details
            };
        }
    }
}
