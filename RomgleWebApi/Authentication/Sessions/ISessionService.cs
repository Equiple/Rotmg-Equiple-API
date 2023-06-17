namespace RotmgleWebApi.Authentication
{
    public interface ISessionService
    {
        Task<Session?> GetSessionAsync(string accessToken);

        Task AddSessionAsync(Session session);

        Task<IEnumerable<Session>> GetUserSessionsAsync(string userId, string deviceId);

        Task RemoveUserSessionsAsync(string userId, string deviceId);
    }
}
