using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using RotmgleWebApi.ModelBinding;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace RotmgleWebApi.Authentication
{
    public class TokenAuthenticationHandler : AuthenticationHandler<TokenAuthenticationOptions>
    {
        private readonly IDeviceIdProviderCollection _deviceIdProviderCollection;
        private readonly IAccessTokenService _accessTokenService;

        public TokenAuthenticationHandler(
            IOptionsMonitor<TokenAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IDeviceIdProviderCollection deviceIdProviderCollection,
            IAccessTokenService accessTokenService)
            : base(options, logger, encoder, clock)
        {
            _deviceIdProviderCollection = deviceIdProviderCollection;
            _accessTokenService = accessTokenService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string authorizationHeader = Request.Headers.Authorization;
            if (string.IsNullOrWhiteSpace(authorizationHeader))
            {
                return AuthenticateResult.Fail("Unauthorized");
            }
            Match regexMatch = Regex.Match(authorizationHeader, @"(?<=^[Bb]earer\s+)\S+$");
            if (!regexMatch.Success)
            {
                return AuthenticateResult.Fail("Unauthorized");
            }
            string accessToken = regexMatch.Value;

            string deviceId = _deviceIdProviderCollection.GetFirstDefinedOrDefaultDeviceId(Context);
            bool isRefreshTokenRequest = Options.RefreshTokenRequestPath != null
                && Request.Path.Value is string path
                && path.StartsWith($"/{Options.RefreshTokenRequestPath}");

            Result<IEnumerable<Claim>> validationResult = await _accessTokenService.ValidateAccessTokenAsync(
                accessToken,
                deviceId,
                validateLifetime: !isRefreshTokenRequest);
            IEnumerable<Claim> claims;
            switch (validationResult)
            {
                case Result<IEnumerable<Claim>>.Ok ok:
                    claims = ok.Value;
                    break;
                case Result<IEnumerable<Claim>>.Error error:
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
