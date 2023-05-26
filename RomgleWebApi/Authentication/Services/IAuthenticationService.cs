namespace RotmgleWebApi.Authentication
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> AuthenticateGuestAsync();

        Task<Result<AuthenticationResult>> AuthenticateAsync(
            string playerId,
            string? deviceId,
            AuthenticationPermit permit);

        Task<Result<AuthenticationResult>> RefreshAccessTokenAsync(
            string playerId,
            string? deviceId,
            string refreshToken);

        Task LogoutAsync(string playerId, string? deviceId);
    }
}
