using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.Services
{
    public interface IAccessTokenService
    {
        string GenerateAccessToken(string playerId);

        Task<RefreshToken> GenerateRefreshToken();

        bool ValidateAccessTokenIgnoringLifetime(string authorizationHeader);
    }
}
