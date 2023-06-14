using RotmgleWebApi.Authentication;

namespace RotmgleWebApi.AuthenticationImplementation
{
    public class TokenAuthenticationRequest
    {
        public TokenAuthenticationResultType ResultType { get; set; }

        public IdentityProvider Provider { get; set; }

        public string? IdToken { get; set; }

        public string? AuthCode { get; set; }
    }
}
