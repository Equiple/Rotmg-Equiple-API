namespace RomgleWebApi.Services.Implementations
{
    public class JobService : IJobService
    {
        private readonly IPlayerService _playerService;
        private readonly IRefreshTokenService _refreshTokenService;

        public JobService(IPlayerService playerService,
            IRefreshTokenService refreshTokenService)
        {
            _playerService = playerService;
            _refreshTokenService = refreshTokenService;
        }

        public Task InvalidateExpiredDailyGamesAsync()
        {
            return _playerService.InvalidateExpiredDailyGamesAsync();
        }

        public async Task RemoveExpiredTokensAndGuestsAsync()
        {
            await _refreshTokenService.RemoveExpiredTokensAsync();
            await _playerService.RemoveInactiveGuestAccountsAsync();
        }
    }
}
