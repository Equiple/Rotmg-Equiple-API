using RotmgleWebApi.Players;

namespace RotmgleWebApiTests.Mocks
{
    internal interface IPlayerServiceMock
    {
        IReadOnlyList<Player> Players { get; }

        void SetInitialPlayers(params Player[] initialPlayers);

        void SetInitialPlayers(IEnumerable<Player> initialPlayers);
    }
}
