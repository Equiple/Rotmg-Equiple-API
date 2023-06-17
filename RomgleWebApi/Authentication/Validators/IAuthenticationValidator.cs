namespace RotmgleWebApi.Authentication
{
    public interface IAuthenticationValidator<TIdentityProvider>
        where TIdentityProvider : struct, Enum
    {
        TIdentityProvider IdentityProvider { get; }

        Task<Result<AuthenticationValidatorResult<TIdentityProvider>>> ValidateAsync(
            AuthenticationPermit<TIdentityProvider> permit);
    }
}
