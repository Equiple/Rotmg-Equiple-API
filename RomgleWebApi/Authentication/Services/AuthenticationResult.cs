namespace RotmgleWebApi.Authentication
{
    public readonly record struct AuthenticationResult(string AccessToken, string RefreshToken);
}
