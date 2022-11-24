using MongoDB.Driver.Linq;
using MongoDB.Driver;
using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.Services
{
    public interface IRefreshTokenService
    {
        Task CreateAsync(RefreshToken token);

        Task UpdateAsync(RefreshToken token);

        Task<RefreshToken?> GetTokenOrDefaultAsync(string refreshToken);

        Task RevokeRefreshTokens(string deviceId);

        Task<bool> DoesExistAsync(string refreshToken);

        Task RemoveExpiredTokensAsync();
    }
}
