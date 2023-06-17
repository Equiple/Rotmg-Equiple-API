using RotmgleWebApi.Authentication;

namespace RotmgleWebApi.AuthenticationImplementation
{
    public class RealmeyeAuthenticationValidator : IAuthenticationValidator<IdentityProvider>
    {
        public IdentityProvider IdentityProvider => IdentityProvider.Realmeye;

        public Task<Result<AuthenticationValidatorResult<IdentityProvider>>> ValidateAsync(
            AuthenticationPermit<IdentityProvider> permit)
        {
            throw new NotImplementedException();
        }
    }
}
