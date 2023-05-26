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

        public string GenerateAccessTokenAsync(string playerId)
        {
            ClaimsIdentity subject = new(new[]
            {
                new Claim(CustomClaimNames.UserId, playerId),
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
            JwtSecurityToken token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
            string serializedToken = tokenHandler.WriteToken(token);

            return serializedToken;
        }

        public RefreshToken GenerateRefreshTokenAsync()
        {
            byte[] tokenBytes = RandomNumberGenerator.GetBytes(_settings.RefreshTokenByteLength);
            RefreshToken token = new()
            {
                Token = Convert.ToBase64String(tokenBytes),
                Expires = DateTime.UtcNow.AddDays(_settings.RefreshTokenLifetimeDays),
            };
            return token;
        }

        public Result<ClaimsPrincipal> ValidateAccessTokenAsync(string accessToken)
        {
            JwtSecurityTokenHandler tokenHandler = new();
            SymmetricSecurityKey securityKey = new(Encoding.ASCII.GetBytes(_settings.SecretKey));
            TokenValidationParameters validationParams = new()
            {
                ValidateLifetime = true,
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

            try
            {
                ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(accessToken, validationParams, out _);
                return claimsPrincipal;
            }
            catch (SecurityTokenException e)
            {
                _logger.LogError(e, "Token validation exception");
                return new Exception("Invalid token");
            }
        }
    }
}
