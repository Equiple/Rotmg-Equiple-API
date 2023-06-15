namespace RotmgleWebApi.Authentication
{
    public interface IUserService<TIdentityProvider>
        where TIdentityProvider : struct, Enum
    {
        Task<User<TIdentityProvider>?> GetUserAsync(string id);

        Task<User<TIdentityProvider>?> GetUserAsync(Identity<TIdentityProvider> identity);

        Task<User<TIdentityProvider>> CreateUserAsync(string? name, Identity<TIdentityProvider>? identity);

        Task UpdateUserAsync(User<TIdentityProvider> user);

        Task<IReadOnlyDictionary<string, string>> CreateSessionPayloadAsync(string userId);
    }
}
