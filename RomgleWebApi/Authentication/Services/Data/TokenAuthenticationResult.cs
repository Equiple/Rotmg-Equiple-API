namespace RotmgleWebApi.Authentication
{
    public readonly record struct TokenAuthenticationResult(
        TokenAuthenticationResultType Type,
        string? AccessToken = null,
        string? RefreshToken = null);
}
