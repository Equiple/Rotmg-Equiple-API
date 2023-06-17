using RotmgleWebApi.Authentication;

namespace RotmgleWebApi.AuthenticationImplementation
{
    public class TokenAuthenticationResponse
    {
        public bool IsAuthenticated { get; set; }

        public TokenAuthenticationResultType? Type { get; set; }

        public string? AccessToken { get; set; }

        public string? RefreshToken { get; set; }
    }
}
