using RomgleWebApi.Data.Models.Auth;

namespace RotmgleWebApiTests.Mocks.Services
{
    internal interface IRefreshTokenServiceMock
    {
        IReadOnlyList<RefreshToken> RefreshTokens { get; }

        void SetInitialRefreshTokens(params RefreshToken[] initialRefreshTokens);

        void SetInitialRefreshTokens(IEnumerable<RefreshToken> initialRefreshTokens);
    }
}
