using RomgleWebApi.Data.Auth;
using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.IdentityValidators
{
    public interface IAuthenticationValidator
    {
        IdentityProvider IdentityProvider { get; }

        Task<AuthenticationValidatorResult> Validate(AuthenticationPermit permit);
    }
}
