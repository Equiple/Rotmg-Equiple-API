namespace RotmgleWebApi.Authentication
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> AuthenticateGuestAsync(string deviceId);

        Task<Result<AuthenticationResult>> AuthenticateAsync(
            string? loggedPlayerId,
            string deviceId,
            AuthenticationPermit permit);

        Task<Result<AuthenticationResult>> RefreshAccessTokenAsync(
            string playerId,
            string deviceId,
            string refreshToken);

        Task LogoutAsync(string playerId, string deviceId);
    }
}
