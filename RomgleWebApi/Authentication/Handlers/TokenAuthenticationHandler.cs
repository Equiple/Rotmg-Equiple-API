using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace RotmgleWebApi.Authentication
{
    public class TokenAuthenticationHandler<TIdentityProvider> : AuthenticationHandler<TokenAuthenticationOptions>
        where TIdentityProvider : struct, Enum
    {
        private readonly ITokenAuthenticationService<TIdentityProvider> _service;

        public TokenAuthenticationHandler(
            IOptionsMonitor<TokenAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            ITokenAuthenticationService<TIdentityProvider> authenticationService)
            : base(options, logger, encoder, clock)
        {
            _service = authenticationService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            Result<IEnumerable<Claim>, AccessTokenValidationException> validationResult =
                await _service.ValidateAccessTokenAsync(Context);
            IEnumerable<Claim> claims;
            switch (validationResult)
            {
                case Result<IEnumerable<Claim>, AccessTokenValidationException>.Ok ok:
                    claims = ok.Value;
                    break;

                case Result<IEnumerable<Claim>, AccessTokenValidationException>.Error error:
                    if (error.Exception.TokenNotFound)
                    {
                        return AuthenticateResult.NoResult();
                    }
                    return AuthenticateResult.Fail(error.Exception);

                default:
                    throw new NotSupportedException();
            }

            ClaimsIdentity identity = new(claims, Scheme.Name);
            ClaimsPrincipal principal = new(identity);
            AuthenticationTicket ticket = new(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
    }
}
