using RotmgleWebApi.Players;

namespace RotmgleWebApi.Jobs
{
    public class JobService : IJobService
    {
        private readonly IPlayerService _playerService;

        public JobService(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        public Task InvalidateExpiredDailyGamesAsync()
        {
            return _playerService.InvalidateExpiredDailyGamesAsync();
        }

        public Task RemoveInactiveGuestsAsync()
        {
            return _playerService.RemoveInactiveGuestAccountsAsync();
        }
    }
}
