using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RotmgleWebApi.Authentication;

namespace RotmgleWebApi.AuthenticationImplementation
{
    public class SessionService : ISessionService
    {
        private readonly IMongoCollection<Session> _sessionCollection;

        public SessionService(IOptions<TokenAuthenticationStorageOptions> options)
        {
            _sessionCollection = MongoUtils.GetCollection<Session>(options.Value, options.Value.SessionCollectionName);
        }

        public async Task<Authentication.Session?> GetSessionAsync(string accessToken)
        {
            Session? sessionModel = await _sessionCollection
                .Find(s => s.AccessToken == accessToken)
                .FirstOrDefaultAsync();
            return sessionModel?.ToSession();
        }

        public Task AddSessionAsync(Authentication.Session session)
        {
            return _sessionCollection.InsertOneAsync(session.ToSessionModel());
        }

        public async Task<IEnumerable<Authentication.Session>> GetUserSessionsAsync(string userId, string deviceId)
        {
            List<Session> sessionModels = await _sessionCollection
                .Find(s => s.UserId == userId && s.DeviceId == deviceId)
                .ToListAsync();
            return sessionModels.Select(model => model.ToSession()).ToList();
        }

        public Task RemoveUserSessionsAsync(string userId, string deviceId)
        {
            return _sessionCollection.DeleteManyAsync(s => s.UserId == userId && s.DeviceId == deviceId);
        }
    }
}
