using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace RotmgleWebApi.Authorization
{
    public class ExpirationAuthorizationHandler : AuthorizationHandler<ExpirationAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ExpirationAuthorizationRequirement requirement)
        {
            string expiration = context.User.FindFirstValue(requirement.ClaimType);
            if (!long.TryParse(expiration, out long expirationUnixSeconds))
            {
                return Task.CompletedTask;
            }
            DateTime expires = DateTimeOffset.FromUnixTimeSeconds(expirationUnixSeconds).DateTime;
            if (DateTime.UtcNow >= expires)
            {
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
