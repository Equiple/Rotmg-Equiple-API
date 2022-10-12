using Microsoft.AspNetCore.Authorization;

namespace RomgleWebApi.Authorization.Requirements
{
    public class ExpirationAuthorizationRequirement : IAuthorizationRequirement
    {
        public ExpirationAuthorizationRequirement(string expirationClaimType)
        {
            ClaimType = expirationClaimType;
        }

        public string ClaimType { get; }
    }
}
