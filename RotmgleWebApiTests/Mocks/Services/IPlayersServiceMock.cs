using RomgleWebApi.Player.Models;

namespace RotmgleWebApiTests.Mocks.Services
{
    internal interface IPlayersServiceMock
    {
        IReadOnlyList<Player> Players { get; }

        void SetInitialPlayers(params Player[] initialPlayers);

        void SetInitialPlayers(IEnumerable<Player> initialPlayers);
    }
}
