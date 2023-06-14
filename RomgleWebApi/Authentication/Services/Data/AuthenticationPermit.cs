namespace RotmgleWebApi.Authentication
{
    public readonly record struct AuthenticationPermit<TIdentityProvider>(
        TokenAuthenticationResultType ResultType,
        TIdentityProvider Provider,
        string? IdToken = null,
        string? AuthCode = null)
        where TIdentityProvider : struct, Enum;
}
