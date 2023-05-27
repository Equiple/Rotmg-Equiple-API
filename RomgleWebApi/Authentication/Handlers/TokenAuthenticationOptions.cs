using Microsoft.AspNetCore.Authentication;

namespace RotmgleWebApi.Authentication
{
    public class TokenAuthenticationOptions : AuthenticationSchemeOptions
    {
        public string? RefreshTokenRequestPath { get; set; }
    }
}
