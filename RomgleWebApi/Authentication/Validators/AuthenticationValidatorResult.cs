namespace RotmgleWebApi.Authentication
{
    public readonly record struct AuthenticationValidatorResult<TIdentityProvider>(
        Identity<TIdentityProvider> Identity,
        string? Name)
        where TIdentityProvider : struct, Enum;
}
