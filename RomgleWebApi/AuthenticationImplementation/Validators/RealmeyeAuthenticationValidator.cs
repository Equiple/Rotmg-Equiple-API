using Microsoft.Extensions.Options;
using RotmgleWebApi.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace RotmgleWebApi.AuthenticationImplementation
{
    public class RealmeyeAuthenticationValidator : IAuthenticationValidator<IdentityProvider>
    {
        private readonly RealmeyeAuthenticationOptions _options;

        public RealmeyeAuthenticationValidator(IOptions<RealmeyeAuthenticationOptions> options)
        {
            _options = options.Value;
        }

        public IdentityProvider IdentityProvider => IdentityProvider.Realmeye;

        public async Task<Result<AuthenticationValidatorResult<IdentityProvider>>> ValidateAsync(
            AuthenticationPermit<IdentityProvider> permit)
        {
            if (permit.AuthCode == null)
            {
                return new Exception("Only auth code flow is supported");
            }

            using HttpClient client = new();
            HttpResponseMessage httpResponse = await client.PostAsJsonAsync(
                $"{_options.Uri}/getToken",
                new RealmeyeTokenRequest { AuthCode = permit.AuthCode });

            if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                return new Exception("Invalid auth code");
            }

            if (!httpResponse.IsSuccessStatusCode)
            {
                return new Exception("Couldn't get realmeye id token");
            }

            RealmeyeTokenResponse? response =
                await httpResponse.Content.ReadFromJsonAsync<RealmeyeTokenResponse>();

            if (response == null)
            {
                return new Exception("Couldn't deserialize realmeye token response");
            }

            JwtSecurityTokenHandler jwtHandler = new();
            
            if (!jwtHandler.CanReadToken(response.IdToken))
            {
                return new Exception("Id token is not a valid JWT");
            }

            JwtSecurityToken jwt = jwtHandler.ReadJwtToken(response.IdToken);
            Claim? idClaim = jwt.Claims.FirstOrDefault(claim => claim.Type == "uid");
            Claim? nameClaim = jwt.Claims.FirstOrDefault(claim => claim.Type == "unm");

            if (idClaim == null || nameClaim == null)
            {
                return new Exception("Id token doesn't contain id or name claim(s)");
            }

            Identity<IdentityProvider> identity = new(IdentityProvider.Realmeye, idClaim.Value);
            return new AuthenticationValidatorResult<IdentityProvider>(identity, nameClaim.Value);
        }
    }
}
