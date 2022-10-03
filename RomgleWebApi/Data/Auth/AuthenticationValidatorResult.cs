using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.Data.Auth
{
    public struct AuthenticationValidatorResult
    {
        public AuthenticationValidatorResult(Identity identity)
        {
            Identity = identity;
        }

        public readonly static AuthenticationValidatorResult Invalid = new AuthenticationValidatorResult();

        public bool IsValid => Identity != null;

        public Identity? Identity { get; }
    }
}
