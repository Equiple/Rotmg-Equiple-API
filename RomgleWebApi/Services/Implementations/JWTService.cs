using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Models.Auth;
using RomgleWebApi.Data.Settings;
using RomgleWebApi.Utils;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RomgleWebApi.Services.Implementations
{
    public class JWTService : IAccessTokenService
    {
        private readonly IPlayersService _playersService;
        private readonly TokenAuthorizationSettings _authorizationSettings;

        public JWTService(
            IPlayersService playersService,
            IOptions<TokenAuthorizationSettings> authorizationSettings)
        {
            _playersService = playersService;
            _authorizationSettings = authorizationSettings.Value;
        }

        public async Task<string> GenerateAccessTokenAsync(string playerId)
        {
            Player player = await _playersService.GetAsync(playerId);
            ClaimsIdentity subject = new ClaimsIdentity(new[]
            {
                new Claim(CustomClaimNames.UserId, playerId)
            });
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = subject,
                Issuer = _authorizationSettings.Issuer,
                Audience = _authorizationSettings.Audience,
                Expires = DateTime.UtcNow.AddMinutes(_authorizationSettings.AccessTokenLifetimeMinutes),
                SigningCredentials = new SigningCredentials(
                    SecurityUtils.GetSecurityKey(player.SecretKey),
                    SecurityAlgorithms.HmacSha256)
            };
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
            string serializedToken = tokenHandler.WriteToken(token);

            return serializedToken;
        }

        public async Task<RefreshToken> GenerateRefreshTokenAsync()
        {
            string tokenValue;
            bool alreadyExists;
            do
            {
                tokenValue = SecurityUtils.GenerateBase64SecurityKey();
                Player? player = await _playersService.GetByRefreshTokenAsync(tokenValue);
                alreadyExists = player != null;
            }
            while (alreadyExists);

            RefreshToken token = new RefreshToken
            {
                Token = tokenValue,
                Expires = DateTime.UtcNow.AddDays(_authorizationSettings.RefreshTokenLifetimeDays)
            };
            return token;
        }

        public async Task<ClaimsPrincipal?> ValidateAccessTokenAsync(string accessToken, bool ignoreExpiration)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            if (!tokenHandler.CanReadToken(accessToken))
            {
                return null;
            }
            JwtSecurityToken jwt = tokenHandler.ReadJwtToken(accessToken);
            Claim? userIdClaim = jwt.Claims.FirstOrDefault(claim => claim.Type == CustomClaimNames.UserId);
            if (userIdClaim == null)
            {
                return null;
            }
            Player player = await _playersService.GetAsync(userIdClaim.Value);
            TokenValidationParameters validationParams = new TokenValidationParameters
            {
                ValidateIssuer = _authorizationSettings.ValidateIssuer,
                ValidIssuer = _authorizationSettings.Issuer,

                ValidateAudience = _authorizationSettings.ValidateAudience,
                ValidAudience = _authorizationSettings.Audience,

                ValidateLifetime = !ignoreExpiration,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = SecurityUtils.GetSecurityKey(player.SecretKey)
            };

            try
            {
                ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(accessToken, validationParams, out _);
                return claimsPrincipal;
            }
            catch (SecurityTokenException)
            {
                return null;
            }
        }
    }
}
