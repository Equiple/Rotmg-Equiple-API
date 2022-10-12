using RomgleWebApi.Data.Auth;
using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.Authentication.AuthenticationValidators
{
    public interface IAuthenticationValidator
    {
        IdentityProvider IdentityProvider { get; }

        Task<AuthenticationValidatorResult> Validate(AuthenticationPermit permit);
    }
}
