using MongoDB.Driver;
using RotmgleWebApi.Authentication;
using RotmgleWebApi.Players;

namespace RotmgleWebApi.AuthenticationImplementation
{
    public class UserService : IUserService<IdentityProvider>
    {
        private readonly IPlayerService _playerService;

        public UserService(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        public async Task<User<IdentityProvider>?> GetUserAsync(string id)
        {
            Player? player = await _playerService.GetOrDefaultAsync(id);
            return player?.ToUser();
        }

        public async Task<User<IdentityProvider>?> GetUserAsync(Identity<IdentityProvider> identity)
        {
            Player? player = await _playerService.GetByIdentityAsync(identity.ToIdentityModel());
            return player?.ToUser();
        }

        public async Task<User<IdentityProvider>> CreateUserAsync(
            string? name,
            Identity<IdentityProvider>? identity)
        {
            Player player = await _playerService.CreateNewAsync(name, identity?.ToIdentityModel());
            return player.ToUser();
        }

        public async Task AddUserIdentityAsync(
            string userId,
            Identity<IdentityProvider> identity,
            string? name)
        {
            Player player = await _playerService.GetAsync(userId);
            player.Identities.Add(identity.ToIdentityModel());
            if (name != null && player.Role == "guest")
            {
                player.Name = name;
            }
            player.Role = "user";
            await _playerService.UpdateAsync(player);
        }

        public async Task<IReadOnlyDictionary<string, string>> CreateSessionPayloadAsync(string userId)
        {
            Player player = await _playerService.GetAsync(userId);
            Dictionary<string, string> payload = new()
            {
                { "role", player.Role },
            };
            return payload;
        }
    }
}
