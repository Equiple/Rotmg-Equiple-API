using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Models.Auth;
using RomgleWebApi.Data.Settings;
using RomgleWebApi.Utils;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace RomgleWebApi.Services.Implementations
{
    public class AccessTokenService : IAccessTokenService
    {
        private readonly IPlayersService _playersService;
        private readonly TokenAuthorizationSettings _authorizationSettings;

        public AccessTokenService(
            IPlayersService playersService,
            IOptions<TokenAuthorizationSettings> authorizationSettings)
        {
            _playersService = playersService;
            _authorizationSettings = authorizationSettings.Value;
        }

        public string GenerateAccessToken(string playerId)
        {
            ClaimsIdentity subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimNames.UserId, playerId)
            });
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = subject,
                Issuer = _authorizationSettings.Issuer,
                Audience = _authorizationSettings.Audience,
                Expires = DateTime.UtcNow.AddMinutes(_authorizationSettings.AccessTokenLifetimeMinutes),
                SigningCredentials = new SigningCredentials(_authorizationSettings.GetSecurityKey(), SecurityAlgorithms.HmacSha256)
            };
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
            string serializedToken = tokenHandler.WriteToken(token);

            return serializedToken;
        }

        public async Task<RefreshToken> GenerateRefreshToken()
        {
            const int tokenByteCount = 64;
            string tokenValue;
            bool alreadyExists;
            do
            {
                tokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(tokenByteCount));
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

        public bool ValidateAccessTokenIgnoringLifetime(string authorizationHeader)
        {
            //Regex.Match(authorizationHeader, @"(?<=^Bearer\s+)(\S+)$");
            TokenValidationParameters validationParams = new TokenValidationParameters
            {
                ValidateIssuer = _authorizationSettings.ValidateIssuer,
                ValidIssuer = _authorizationSettings.Issuer,

                ValidateAudience = _authorizationSettings.ValidateAudience,
                ValidAudience = _authorizationSettings.Audience,

                ValidateLifetime = false,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _authorizationSettings.GetSecurityKey()
            };
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                tokenHandler.ValidateToken(authorizationHeader, validationParams, out _);
                return true;
            }
            catch (SecurityTokenException)
            {
                return false;
            }
        }
    }
}
