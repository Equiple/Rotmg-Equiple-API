namespace RotmgleWebApi.Authentication
{
    public record class Identity<TIdentityProvider>(TIdentityProvider Provider, string Id)
        where TIdentityProvider : struct, Enum;
}
