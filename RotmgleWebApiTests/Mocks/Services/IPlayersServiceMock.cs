using RotmgleWebApi.Players;

namespace RotmgleWebApiTests.Mocks.Services
{
    internal interface IPlayerServiceMock
    {
        IReadOnlyList<Player> Players { get; }

        void SetInitialPlayers(params Player[] initialPlayers);

        void SetInitialPlayers(IEnumerable<Player> initialPlayers);
    }
}
