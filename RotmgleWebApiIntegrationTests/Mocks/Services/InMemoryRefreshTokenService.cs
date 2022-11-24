using RomgleWebApi.Data.Models.Auth;
using RomgleWebApi.Extensions;
using RomgleWebApi.Services;


namespace RotmgleWebApiTests.Mocks.Services
{
    internal class InMemoryRefreshTokenService : IRefreshTokenServiceMock, IRefreshTokenService
    {
        private readonly List<RefreshToken> _refreshTokens = new List<RefreshToken>();

        public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens;

        public void SetInitialRefreshTokens(params RefreshToken[] initialPlayers)
        {
            SetInitialRefreshTokens((IEnumerable<RefreshToken>)initialPlayers);
        }

        public void SetInitialRefreshTokens(IEnumerable<RefreshToken> initialRefreshTokens)
        {
            _refreshTokens.Clear();
            _refreshTokens.AddRange(initialRefreshTokens);
        }

        public async Task CreateAsync(RefreshToken token)
        {
            if(await DoesExistAsync(token.Token))
            {
                throw new Exception($"Refresh token for {token.DeviceId}:{token.Token} already exists");
            }
            _refreshTokens.Add(token);
        }

        public Task<bool> DoesExistAsync(string refreshToken)
        {
            return Task.FromResult(_refreshTokens.FindIndex(token => token.Token == refreshToken) >= 0);
        }

        public Task<RefreshToken?> GetTokenOrDefaultAsync(string refreshToken)
        {
            int temp = _refreshTokens.FindIndex(token => token.Token == refreshToken);
            RefreshToken? token = null;
            if (temp >= 0)
            {
                token = _refreshTokens[temp];
            }
            return Task.FromResult(token);
        }

        public Task RemoveExpiredTokensAsync()
        {
            _refreshTokens.RemoveAll(token => token.Expires.Date < DateTime.UtcNow.Date);
            return Task.CompletedTask;
        }

        public Task RevokeRefreshTokens(string deviceId)
        {
            foreach(RefreshToken token in _refreshTokens.Where(token => token.DeviceId == deviceId))
            {
                token.Revoke();
            }
            return Task.CompletedTask;
        }

        public async Task UpdateAsync(RefreshToken refreshToken)   
        {
            if (await DoesExistAsync(refreshToken.Token))
            {
                int index = _refreshTokens.FindIndex(token => token == refreshToken);
                _refreshTokens[index] = refreshToken;
            }
            else throw new Exception($"Refresh token for {refreshToken.DeviceId}:{refreshToken.Token} does not exist");
        }
    }
}
