using RomgleWebApi.Data.Auth;
using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.Services
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> AuthenticateGuest(string? name);

        Task<AuthenticationResult> Authenticate(AuthenticationPermit permit);

        Task<AuthenticationResult> AddIdentity(string playerId, AuthenticationPermit permit);

        Task<AuthenticationResult> RefreshAccessToken(string playerId, string refreshToken);

        Task Logout(string playerId);
    }
}
