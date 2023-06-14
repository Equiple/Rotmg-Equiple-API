using System.Security.Claims;

namespace RotmgleWebApi.Authentication
{
    public interface ITokenAuthenticationService<TIdentityProvider>
        where TIdentityProvider : struct, Enum
    {
        Task<TokenAuthenticationResult> AuthenticateGuestAsync(
            HttpContext context,
            TokenAuthenticationResultType resultType);

        Task<Result<TokenAuthenticationResult>> AuthenticateAsync(
            HttpContext context,
            AuthenticationPermit<TIdentityProvider> permit);

        Task<Result<IEnumerable<Claim>, AccessTokenValidationException>> ValidateAccessTokenAsync(
            HttpContext context);

        Task<Result<TokenAuthenticationResult>> RefreshAccessTokenAsync(
            HttpContext context,
            string refreshToken);

        Task LogoutAsync(HttpContext context);
    }
}
