using System.Security.Claims;

namespace RotmgleWebApi.Authentication
{
    public interface IAccessTokenService
    {
        string GenerateAccessTokenAsync(string playerId);

        RefreshToken GenerateRefreshTokenAsync();

        Result<ClaimsPrincipal> ValidateAccessTokenAsync(string accessToken);
    }
}
