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

        public async Task<User<IdentityProvider>> CreateUserAsync(string? name, Identity<IdentityProvider> identity)
        {
            Player player = await _playerService.CreateNewAsync(name, identity.ToIdentityModel());
            return player.ToUser();
        }

        public async Task UpdateUserAsync(User<IdentityProvider> user)
        {
            Player player = await _playerService.GetAsync(user.Id);
            player.Name = user.Name;
            player.Identities = user.Identities
                .Select(identity => identity.ToIdentityModel())
                .ToList();
            await _playerService.UpdateAsync(player);
        }

        public Task<IReadOnlyDictionary<string, string>> CreateSessionPayloadAsync(string userId)
        {
            return Task.FromResult<IReadOnlyDictionary<string, string>>(
                new Dictionary<string, string>());
        }
    }
}
