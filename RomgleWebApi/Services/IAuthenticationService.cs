using RomgleWebApi.Data.Auth;
using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.Services
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> AuthenticateGuestAsync();

        Task<AuthenticationResult> AuthenticateAsync(
            AuthenticationPermit permit,
            string? playerId,
            string? deviceId);

        Task<AuthenticationResult> RefreshAccessTokenAsync(
            string playerId,
            string deviceId,
            string refreshToken);

        Task LogoutAsync(string playerId, string deviceId);
    }
}
