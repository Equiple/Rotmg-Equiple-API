namespace RotmgleWebApi.Authentication
{
    public class AuthenticationPermit
    {
        public IdentityProvider Provider { get; set; }

        public string IdToken { get; set; }
    }
}
