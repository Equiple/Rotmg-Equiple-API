using RotmgleWebApi.Players;
using System.Security.Claims;

namespace RotmgleWebApi.Authentication
{
    public interface IAccessTokenService
    {
        string GenerateAccessToken(Player player, string deviceId);

        RefreshToken GenerateRefreshToken();

        Task<Result<IEnumerable<Claim>>> ValidateAccessTokenAsync(
            string accessToken,
            string deviceId,
            bool validateLifetime = true);
    }
}
