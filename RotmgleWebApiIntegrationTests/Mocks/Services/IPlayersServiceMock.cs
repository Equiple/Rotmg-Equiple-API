using RomgleWebApi.Data.Models;

namespace RotmgleWebApiTests.Mocks.Services
{
    internal interface IPlayersServiceMock
    {
        void SetInitialPlayers(params Player[] initialPlayers);

        void SetInitialPlayers(IEnumerable<Player> initialPlayers);
    }
}
