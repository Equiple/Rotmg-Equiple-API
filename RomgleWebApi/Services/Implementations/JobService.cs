namespace RomgleWebApi.Services.Implementations
{
    public class JobService : IJobService
    {
        private readonly IPlayerService _playerService;
        private readonly IAccessTokenService _accessTokenService;

        public JobService(IPlayerService playerService,
            IAccessTokenService accessTokenService)
        {
            _playerService = playerService;
            _accessTokenService = accessTokenService;
        }

        public async Task RemoveExpiredTokensAndGuestsAsync()
        {
            await _accessTokenService.RemoveExpiredRefreshTokensAsync();
            await _playerService.RemoveInactiveGuestAccountsAsync();
        }
    }
}
