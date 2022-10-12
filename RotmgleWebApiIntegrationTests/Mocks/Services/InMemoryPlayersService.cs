using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Models.Auth;
using RomgleWebApi.Services;
using RomgleWebApi.Utils;

namespace RotmgleWebApiTests.Mocks.Services
{
    internal class InMemoryPlayersService : IPlayersServiceMock, IPlayersService
    {
        private readonly List<Player> _players = new List<Player>();

        public void SetInitialPlayers(params Player[] initialPlayers)
        {
            SetInitialPlayers((IEnumerable<Player>)initialPlayers);
        }

        public void SetInitialPlayers(IEnumerable<Player> initialPlayers)
        {
            _players.Clear();
            _players.AddRange(initialPlayers);
        }

        public Task<Player> CreateNewPlayerAsync(Identity identity)
        {
            Player? existingPlayer = GetByIdentityAsync(identity).Result;
            if (existingPlayer != null)
            {
                throw new Exception($"Player with given identity {identity.Provider}:{identity.Id} already exists");
            }

            Player newPlayer = new Player
            {
                Id = Guid.NewGuid().ToString(),
                Identities = new List<Identity> { identity },
                RefreshTokens = new List<RefreshToken>(),
                SecretKey = SecurityUtils.GenerateBase64SecurityKey(),
                NormalStats = new GameStatistic(),
                DailyStats = new GameStatistic(),
                RegistrationDate = DateTime.UtcNow
            };
            _players.Add(newPlayer);

            return Task.FromResult(newPlayer);
        }

        public async Task RefreshSecretKeyAsync(string playerId)
        {
            Player player = await GetAsync(playerId);
            player.SecretKey = SecurityUtils.GenerateBase64SecurityKey();
            await UpdateAsync(player);
        }

        public Task<Player> GetAsync(string id)
        {
            return Task.FromResult(_players.First(player => player.Id == id));
        }

        public Task<Player?> GetByIdentityAsync(Identity identity)
        {
            return Task.FromResult(_players
                .FirstOrDefault(player => player.Identities
                    .Any(playerIdentity =>
                        playerIdentity.Provider == identity.Provider &&
                        playerIdentity.Id == identity.Id)));
        }

        public Task<Player?> GetByRefreshTokenAsync(string refreshToken)
        {
            return Task.FromResult(_players
                .FirstOrDefault(player => player.RefreshTokens
                    .Any(token => token.Token == refreshToken)));
        }

        public Task UpdateAsync(Player updatedPlayer)
        {
            int index = _players.FindIndex(player => player.Id == updatedPlayer.Id);
            _players[index] = updatedPlayer;
            return Task.CompletedTask;
        }

        public Task<bool> WasDailyAttemptedAsync(string id)
        {
            Player player = GetAsync(id).Result;
            return Task.FromResult(player.EndedGames
                .Any(game =>
                    game.Mode == Gamemode.Daily &&
                    game.StartTime.Date == DateTime.UtcNow.Date));
        }
    }
}
