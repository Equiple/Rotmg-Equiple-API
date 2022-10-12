using RomgleWebApi.Authentication.AuthenticationValidators;

namespace RomgleWebApi.Data.Settings
{
    public class AuthenticationServiceSettings
    {
        public List<IAuthenticationValidator> AuthenticationValidators { get; } = new List<IAuthenticationValidator>();
    }
}
