using Microsoft.AspNetCore.Authentication;

namespace RotmgleWebApi.Authentication
{
    public class TokenAuthenticationOptions : AuthenticationSchemeOptions
    {
        public string? RefreshTokenRequestPath { get; set; }

        public int AccessTokenLengthBytes { get; set; } = 64;

        public int AccessTokenLifetimeMinutes { get; set; } = 15;

        public int RefreshTokenLengthBytes { get; set; } = 64;

        public int RefreshTokenLifetimeDays { get; set; } = 7;
    }
}
