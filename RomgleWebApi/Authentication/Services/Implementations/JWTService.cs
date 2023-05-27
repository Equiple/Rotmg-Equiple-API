using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RotmgleWebApi.Authentication
{
    public class JWTService : IAccessTokenService
    {
        private readonly TokenAuthorizationSettings _settings;
        private readonly ILogger<JWTService> _logger;

        public JWTService(
            IOptions<TokenAuthorizationSettings> settings,
            ILogger<JWTService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public string GenerateAccessToken(string playerId, string deviceId)
        {
            ClaimsIdentity subject = new(new[]
            {
                new Claim(CustomClaimNames.UserId, playerId),
                new Claim(CustomClaimNames.DeviceId, deviceId),
            });
            SymmetricSecurityKey securityKey = new(Encoding.ASCII.GetBytes(_settings.SecretKey));
            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = subject,
                Issuer = _settings.Issuer,
                Audience = _settings.Audience,
                Expires = DateTime.UtcNow.AddMinutes(_settings.AccessTokenLifetimeMinutes),
                SigningCredentials = new SigningCredentials(
                    securityKey,
                    SecurityAlgorithms.HmacSha256),
            };
            JwtSecurityTokenHandler tokenHandler = new();
            JwtSecurityToken jwt = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
            string accessToken = tokenHandler.WriteToken(jwt);
            return accessToken;
        }

        public RefreshToken GenerateRefreshToken()
        {
            byte[] tokenBytes = RandomNumberGenerator.GetBytes(_settings.RefreshTokenByteLength);
            RefreshToken token = new()
            {
                Token = Convert.ToBase64String(tokenBytes),
                Expires = DateTime.UtcNow.AddDays(_settings.RefreshTokenLifetimeDays),
            };
            return token;
        }

        public async Task<Result<IEnumerable<Claim>>> ValidateAccessTokenAsync(
            string accessToken,
            string deviceId,
            bool validateLifetime = true)
        {
            JwtSecurityTokenHandler tokenHandler = new();
            SymmetricSecurityKey securityKey = new(Encoding.ASCII.GetBytes(_settings.SecretKey));
            TokenValidationParameters validationParams = new()
            {
                ValidateLifetime = validateLifetime,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,
            };
            if (_settings.Issuer != null)
            {
                validationParams.ValidateIssuer = true;
                validationParams.ValidIssuer = _settings.Issuer;
            }
            if (_settings.Audience != null)
            {
                validationParams.ValidateAudience = true;
                validationParams.ValidAudience = _settings.Audience;
            }
            TokenValidationResult validationResult = await tokenHandler.ValidateTokenAsync(
                accessToken, validationParams);

            if (!validationResult.IsValid)
            {
                _logger.LogError(validationResult.Exception, "Token validation exception");
                return new Exception("Invalid token");
            }

            IEnumerable<Claim> claims = validationResult.ClaimsIdentity.Claims;

            Claim? deviceIdClaim = claims.FirstOrDefault(c => c.Type == CustomClaimNames.DeviceId);
            if (deviceIdClaim != null && deviceIdClaim.Value != deviceId)
            {
                return new Exception("Invalid device id");
            }

            return new Result<IEnumerable<Claim>>.Ok(claims);
        }
    }
}
