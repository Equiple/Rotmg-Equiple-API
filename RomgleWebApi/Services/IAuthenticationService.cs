using RomgleWebApi.Data.Auth;
using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.Services
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> AuthenticateGuestAsync();

        Task<AuthenticationResult> AuthenticateAsync(AuthenticationPermit permit, string? playerId = null);

        Task<AuthenticationResult> RefreshAccessTokenAsync(string playerId, string refreshToken);

        Task LogoutAsync(string playerId);
    }
}
