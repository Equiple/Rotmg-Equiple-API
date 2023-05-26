namespace RotmgleWebApi.Authentication
{
    public class SelfAuthenticationValidator : IAuthenticationValidator
    {
        public IdentityProvider IdentityProvider => IdentityProvider.Self;

        public async Task<Result<AuthenticationValidatorResult>> ValidateAsync(AuthenticationPermit permit)
        {
            return new Exception("Can't authenticate as guest using permit");
        }
    }
}
