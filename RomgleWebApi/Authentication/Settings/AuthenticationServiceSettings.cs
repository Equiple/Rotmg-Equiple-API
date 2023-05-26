namespace RotmgleWebApi.Authentication
{
    public class AuthenticationServiceSettings
    {
        public List<IAuthenticationValidator> AuthenticationValidators { get; } = new List<IAuthenticationValidator>();
    }
}
