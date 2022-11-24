using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using RomgleWebApi.DAL;
using RomgleWebApi.Data.Models.Auth;
using RomgleWebApi.Data.Settings;
using RomgleWebApi.Extensions;

namespace RomgleWebApi.Services.Implementations
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IMongoCollection<RefreshToken> _refreshTokenCollection;

        public RefreshTokenService(IOptions<RotmgleDatabaseSettings> rotmgleDatabaseSettings,
            IDataCollectionProvider dataCollectionProvider)
        {
            _refreshTokenCollection = dataCollectionProvider
                .GetDataCollection<RefreshToken>(rotmgleDatabaseSettings.Value.RefreshTokenCollectionName)
                .AsMongo();
        }

        public async Task CreateAsync(RefreshToken token)
        {
            await _refreshTokenCollection.InsertOneAsync(token);
        }

        public async Task UpdateAsync(RefreshToken token)
        {
            await _refreshTokenCollection.ReplaceOneAsync(refreshToken => refreshToken.Token == token.Token, token);
        }

        public async Task<RefreshToken?> GetTokenOrDefaultAsync(string refreshToken)
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

        public async Task<bool> DoesExistAsync(string refreshToken)
        {
            RefreshToken? refToken = await _refreshTokenCollection
                .Find(token => token.Token == refreshToken).FirstOrDefaultAsync();
            return refToken != null;
        }

        public async Task RemoveExpiredTokensAsync()
        {
            await _refreshTokenCollection.DeleteManyAsync(token => token.Expires.Date < DateTime.UtcNow.Date);
        }
    }
}
