using System.Security.Claims;

namespace RotmgleWebApi.Authentication
{
    public interface IAccessTokenService
    {
        string GenerateAccessToken(string playerId, string deviceId);

        RefreshToken GenerateRefreshToken();

        Task<Result<IEnumerable<Claim>>> ValidateAccessTokenAsync(
            string accessToken,
            string deviceId,
            bool validateLifetime = true);
    }
}
