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

        public async Task RemoveExpiredTokensAndGuestsAsync()
        {
            await _refreshTokenService.RemoveExpiredTokensAsync();
            await _playerService.RemoveInactiveGuestAccountsAsync();
        }
    }
}
