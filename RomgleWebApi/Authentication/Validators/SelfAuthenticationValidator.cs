using RomgleWebApi.Data.Auth;
using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.Authentication.AuthenticationValidators
{
    public class SelfAuthenticationValidator : IAuthenticationValidator
    {
        public IdentityProvider IdentityProvider => IdentityProvider.Self;

        public Task<AuthenticationValidatorResult> Validate(AuthenticationPermit identity)
        {
            //impossible to re-authenticate as self-provided identity (aka guest)
            return Task.FromResult(AuthenticationValidatorResult.Invalid);
        }
    }
}
