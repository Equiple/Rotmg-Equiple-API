namespace RotmgleWebApi.Authentication
{
    public interface IAuthenticationValidator
    {
        IdentityProvider IdentityProvider { get; }

        Task<Result<AuthenticationValidatorResult>> ValidateAsync(AuthenticationPermit permit);
    }
}
