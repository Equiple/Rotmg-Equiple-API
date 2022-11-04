using RomgleWebApi.Data.Models.Auth;
using System.Security.Claims;

namespace RomgleWebApi.Services
{
    public interface IAccessTokenService
    {
        Task<string> GenerateAccessTokenAsync(string playerId, string deviceId);

        Task<RefreshToken> GenerateRefreshTokenAsync(string deviceId);

        Task<ClaimsPrincipal?> ValidateAccessTokenAsync(string accessToken, bool ignoreExpiration = false);

        Task UpdateRefreshTokenAsync(RefreshToken token);

        Task<RefreshToken?> GetRefreshTokenOrDefaultAsync(string refreshToken);

        Task RevokeRefreshTokens(string deviceId);

        Task<bool> DoesRefreshTokenExistAsync(string refreshToken);

        Task RemoveExpiredRefreshTokensAsync();
    }
}
