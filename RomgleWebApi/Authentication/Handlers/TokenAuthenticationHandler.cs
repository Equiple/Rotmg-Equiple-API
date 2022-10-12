using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using RomgleWebApi.Authentication.Options;
using RomgleWebApi.Services;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace RomgleWebApi.Authentication.AuthenticationHandlers
{
    public class TokenAuthenticationHandler : AuthenticationHandler<TokenAuthenticationOptions>
    {
        private readonly IAccessTokenService _accessTokenService;

        public TokenAuthenticationHandler(
            IOptionsMonitor<TokenAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IAccessTokenService accessTokenService)
            : base(options, logger, encoder, clock)
        {
            _accessTokenService = accessTokenService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            const string failureMessage = "Unauthorized";

            string authorizationHeader = Request.Headers.Authorization;
            if (string.IsNullOrWhiteSpace(authorizationHeader))
            {
                return AuthenticateResult.Fail(failureMessage);
            }
            Match regexMatch = Regex.Match(authorizationHeader, @"(?<=^[Bb]earer\s+)\S+$");
            if (!regexMatch.Success)
            {
                return AuthenticateResult.Fail(failureMessage);
            }
            string accessToken = regexMatch.Value;
            ClaimsPrincipal? claimsPrincipal = await _accessTokenService.ValidateAccessToken(
                accessToken,
                ignoreExpiration: Options.IgnoreExpiration);
            if (claimsPrincipal == null)
            {
                return AuthenticateResult.Fail(failureMessage);
            }

            AuthenticationTicket ticket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
    }
}
