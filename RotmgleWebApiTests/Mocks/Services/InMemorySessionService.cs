using RotmgleWebApi.Authentication;

namespace RotmgleWebApiTests.Mocks
{
    internal class InMemorySessionService : ISessionService, ISessionServiceMock
    {
        private readonly List<Session> _sessions = new();

        public void Clear()
        {
            _sessions.Clear();
        }

        public Task AddSessionAsync(Session session)
        {
            _sessions.Add(session);
            return Task.CompletedTask;
        }

        public Task<Session?> GetSessionAsync(string accessToken)
        {
            Session? session = _sessions.FirstOrDefault(s => s.AccessToken == accessToken);
            return Task.FromResult(session);
        }

        public Task<IEnumerable<Session>> GetUserSessionsAsync(string userId, string deviceId)
        {
            IEnumerable<Session> sessions = _sessions
                .Where(s => s.UserId == userId && s.DeviceId == deviceId)
                .ToList();
            return Task.FromResult(sessions);
        }

        public Task RemoveUserSessionsAsync(string userId, string deviceId)
        {
            _sessions.RemoveAll(s => s.UserId == userId && s.DeviceId == deviceId);
            return Task.CompletedTask;
        }
    }
}
