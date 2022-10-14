namespace RomgleWebApi.Data.Models.Auth
{
    public class AuthenticationPermit
    {
        public IdentityProvider Provider { get; set; }

        public string IdToken { get; set; }
    }
}
