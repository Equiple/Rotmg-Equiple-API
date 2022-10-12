using Microsoft.AspNetCore.Authentication;

namespace RomgleWebApi.Authentication.Options
{
    public class TokenAuthenticationOptions : AuthenticationSchemeOptions
    {
        public bool IgnoreExpiration { get; set; }
    }
}
