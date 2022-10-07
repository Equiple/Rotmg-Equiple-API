using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.Data.Auth
{
    public readonly struct AuthenticationValidatorResult
    {
        public static AuthenticationValidatorResult Valid(Identity identity)
        {
            return new AuthenticationValidatorResult(identity);
        }

        public readonly static AuthenticationValidatorResult Invalid =
            new AuthenticationValidatorResult();

        private AuthenticationValidatorResult(Identity identity)
        {
            Identity = identity;
        }

        public bool IsValid => Identity != null;

        public Identity? Identity { get; }
    }
}
