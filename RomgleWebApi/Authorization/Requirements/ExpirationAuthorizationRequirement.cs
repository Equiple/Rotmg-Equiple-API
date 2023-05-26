using Microsoft.AspNetCore.Authorization;

namespace RotmgleWebApi.Authorization
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
