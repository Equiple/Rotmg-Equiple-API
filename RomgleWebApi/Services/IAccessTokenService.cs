using RomgleWebApi.Data.Models.Auth;
using System.Security.Claims;

namespace RomgleWebApi.Services
{
    public interface IAccessTokenService
    {
        Task<string> GenerateAccessToken(string playerId);

        Task<RefreshToken> GenerateRefreshToken();

        Task<ClaimsPrincipal?> ValidateAccessToken(string accessToken, bool ignoreExpiration = false);
    }
}
