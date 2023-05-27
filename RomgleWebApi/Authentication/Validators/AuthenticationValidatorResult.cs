namespace RotmgleWebApi.Authentication
{
    public readonly record struct AuthenticationValidatorResult(Identity Identity, string? Name);
}
