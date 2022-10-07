using RomgleWebApi.Data.Models.Auth;

namespace RotmgleWebApiTests.Data.Models.Auth
{
    internal class AuthenticationResponse : IAuthenticationResponse
    {
        public bool IsAuthenticated { get; set; }

        public string? AccessToken { get; set; }

        public string? RefreshToken { get; set; }
    }
}
