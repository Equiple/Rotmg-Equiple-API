namespace RotmgleWebApi.Authentication
{
    public record class User<TIdentityProvider>(
        string Id,
        string Name,
        IReadOnlyList<Identity<TIdentityProvider>> Identities)
        where TIdentityProvider : struct, Enum
    {
        public bool IsGuest => Identities.Count == 0;
    }
}
