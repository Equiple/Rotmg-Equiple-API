﻿using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Models.Auth;
using RomgleWebApi.Data.Settings;
using RomgleWebApi.Extensions;
using RomgleWebApi.Utils;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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

        public async Task<string> GenerateAccessTokenAsync(string playerId, string deviceId)
        {
            SecurityKey securityKey = await GetSecurityKey(playerId, deviceId);
            ClaimsIdentity subject = new ClaimsIdentity(new[]
            {
                new Claim(CustomClaimNames.UserId, playerId),
                new Claim(CustomClaimNames.DeviceId, deviceId)
            });
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = subject,
                Issuer = _authorizationSettings.Issuer,
                Audience = _authorizationSettings.Audience,
                Expires = DateTime.UtcNow.AddMinutes(_authorizationSettings.AccessTokenLifetimeMinutes),
                SigningCredentials = new SigningCredentials(
                    securityKey,
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
                alreadyExists = await _playersService.DoesRefreshTokenExistAsync(tokenValue);
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
            Claim? deviceIdClaim = jwt.Claims.FirstOrDefault(claim => claim.Type == CustomClaimNames.DeviceId);
            if (userIdClaim == null || deviceIdClaim == null)
            {
                return null;
            }
            SecurityKey securityKey = await GetSecurityKey(userIdClaim.Value, deviceIdClaim.Value);
            TokenValidationParameters validationParams = new TokenValidationParameters
            {
                ValidateIssuer = _authorizationSettings.ValidateIssuer,
                ValidIssuer = _authorizationSettings.Issuer,

                ValidateAudience = _authorizationSettings.ValidateAudience,
                ValidAudience = _authorizationSettings.Audience,

                ValidateLifetime = !ignoreExpiration,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey
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

        private async Task<SecurityKey> GetSecurityKey(string playerId, string deviceId)
        {
            Player player = await _playersService.GetAsync(playerId);
            Device device = player.GetDevice(deviceId);
            byte[] secretKey = Encoding.GetEncoding(_authorizationSettings.SecretKeyEncoding)
                .GetBytes(_authorizationSettings.SecretKey);
            byte[] personalKey = Encoding.GetEncoding(device.PersonalKeyEncoding)
                .GetBytes(device.PersonalKey);
            byte[] securityKeyBytes = secretKey.Concat(personalKey).ToArray();
            string securityKey = Convert.ToBase64String(securityKeyBytes);

            return SecurityUtils.GetSecurityKey(securityKey);
        }
    }
}