using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.Data.Auth
{
    public readonly struct AuthenticationValidatorResult
    {
        public static AuthenticationValidatorResult Valid(Identity identity, string? name = null)
        {
            return new AuthenticationValidatorResult(identity, name);
        }

        public readonly static AuthenticationValidatorResult Invalid =
            new AuthenticationValidatorResult();

        private AuthenticationValidatorResult(Identity identity, string? name)
        {
            Identity = identity;
            Name = name;
        }

        public bool IsValid => Identity != null;

        /// <summary>
        /// not null if IsValid == true
        /// </summary>
        public Identity? Identity { get; }

        public string? Name { get; }
    }
}
