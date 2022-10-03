using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RomgleWebApi.Services;

namespace RomgleWebApi.Filters
{
    public class AuthorizeIgnoringLifetimeFilter : IAuthorizationFilter
    {
        private readonly IAccessTokenService _accessTokenService;

        public AuthorizeIgnoringLifetimeFilter(IAccessTokenService accessTokenService)
        {
            _accessTokenService = accessTokenService;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            string authorizationHeader = context.HttpContext.Request.Headers.Authorization;
            if (_accessTokenService.ValidateAccessTokenIgnoringLifetime(authorizationHeader))
            {
                return;
            }

            context.Result = new UnauthorizedResult();
        }
    }
}
