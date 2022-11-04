using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using RomgleWebApi.DAL;
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
        private readonly IPlayerService _playersService;
        private readonly TokenAuthorizationSettings _authorizationSettings;
        private readonly ILogger<JWTService> _logger;
        private readonly IMongoCollection<RefreshToken> _refreshTokenCollection;

        public JWTService(
            IPlayerService playersService,
            IOptions<TokenAuthorizationSettings> authorizationSettings,
            IOptions<RotmgleDatabaseSettings> rotmgleDatabaseSettings,
            IDataCollectionProvider dataCollectionProvider,
            ILogger<JWTService> logger)
        {
            _playersService = playersService;
            _authorizationSettings = authorizationSettings.Value;
            _refreshTokenCollection = dataCollectionProvider
                .GetDataCollection<RefreshToken>(rotmgleDatabaseSettings.Value.RefreshTokenCollectionName)
                .AsMongo();
            _logger = logger;
        }

        #region public methods

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

        public async Task<RefreshToken> GenerateRefreshTokenAsync(string deviceId)
        {
            string tokenValue;
            bool alreadyExists;
            do
            {
                tokenValue = SecurityUtils.GenerateBase64SecurityKey();
                alreadyExists = await DoesRefreshTokenExistAsync(tokenValue);
            }
            while (alreadyExists);
            RefreshToken token = new RefreshToken
            {
                Token = tokenValue,
                Expires = DateTime.UtcNow.AddDays(_authorizationSettings.RefreshTokenLifetimeDays),
                DeviceId = deviceId
            };
            await _refreshTokenCollection.InsertOneAsync(token);
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

            SecurityKey securityKey;
            try
            {
                securityKey = await GetSecurityKey(userIdClaim.Value, deviceIdClaim.Value);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error in {GetType().Name} during access token validation: {nameof(SecurityKey)} creation");
                return null;
            }

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

        public async Task UpdateRefreshTokenAsync(RefreshToken token)
        {
            await _refreshTokenCollection.ReplaceOneAsync(refreshToken => refreshToken.Token == token.Token, token);
        }

        public async Task<RefreshToken?> GetRefreshTokenOrDefaultAsync(string refreshToken)
        {
            IMongoQueryable<RefreshToken> tokens = _refreshTokenCollection.AsQueryable()
                .Where(token => token.Token == refreshToken);
            return await tokens.FirstOrDefaultAsync();
        }

        public async Task RevokeRefreshTokens(string deviceId)
        {
            IMongoQueryable<RefreshToken> tokens = _refreshTokenCollection.AsQueryable()
                .Where(refreshToken => refreshToken.DeviceId == deviceId);
            var updates = new List<WriteModel<RefreshToken>>();
            var filterBuilder = Builders<RefreshToken>.Filter;
            foreach (RefreshToken refreshToken in tokens)
            {
                if (!refreshToken.IsActive())
                {
                    return;
                }
                refreshToken.Revoked = DateTime.UtcNow;
                var filter = filterBuilder.Where(token => token.Token == refreshToken.Token);
                updates.Add(new ReplaceOneModel<RefreshToken>(filter, refreshToken));
            }
            await _refreshTokenCollection.BulkWriteAsync(updates);
        }

        public async Task<bool> DoesRefreshTokenExistAsync(string refreshToken)
        {
            RefreshToken? refToken = await _refreshTokenCollection
                .Find(token => token.Token == refreshToken).FirstOrDefaultAsync();
            return refToken != null;
        }

        public async Task RemoveExpiredRefreshTokensAsync()
        {
            await _refreshTokenCollection.DeleteManyAsync(token => token.Expires.Date < DateTime.UtcNow.Date);
        }

        #endregion public methods

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
