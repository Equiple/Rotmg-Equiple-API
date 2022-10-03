using RomgleWebApi.Data.Auth;
using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.Services
{
    public interface IAuthenticationService
    {
        Task<IAuthenticationResult> AuthenticateGuest(string? name);

        Task<IAuthenticationResult> Authenticate(AuthenticationPermit permit);

        Task<IAuthenticationResult> AddIdentity(string playerId, AuthenticationPermit permit);

        Task<IAuthenticationResult> RefreshAccessToken(string playerId, string refreshToken);
    }
}
