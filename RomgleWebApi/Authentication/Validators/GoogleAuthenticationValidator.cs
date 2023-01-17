using Google.Apis.Auth;
using RomgleWebApi.Data.Auth;
using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.Authentication.Validators
{
    public class GoogleAuthenticationValidator : IAuthenticationValidator
    {
        public IdentityProvider IdentityProvider => IdentityProvider.Google;

        public async Task<AuthenticationValidatorResult> ValidateAsync(AuthenticationPermit permit)
        {
            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(permit.IdToken);
            }
            catch (InvalidJwtException e)
            {
                return AuthenticationValidatorResult.Invalid;
            }

            Identity identity = new Identity
            {
                Provider = IdentityProvider,
                Id = payload.Email
            };
            return AuthenticationValidatorResult.Valid(identity, name: payload.Name);
        }
    }
}
