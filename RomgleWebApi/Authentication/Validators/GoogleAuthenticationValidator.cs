using Google.Apis.Auth;

namespace RotmgleWebApi.Authentication
{
    public class GoogleAuthenticationValidator : IAuthenticationValidator
    {
        public IdentityProvider IdentityProvider => IdentityProvider.Google;

        public async Task<Result<AuthenticationValidatorResult>> ValidateAsync(AuthenticationPermit permit)
        {
            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(permit.IdToken);
            }
            catch (InvalidJwtException e)
            {
                return new Exception("Invalid google id token");
            }

            Identity identity = new()
            {
                Provider = IdentityProvider,
                Id = payload.Email,
            };
            return new AuthenticationValidatorResult(identity, Name: payload.Name);
        }
    }
}
