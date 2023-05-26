namespace RotmgleWebApi.Authentication
{
    public class AuthenticationResponse
    {
        public bool IsAuthenticated { get; set; }

        public string? AccessToken { get; set; }

        public string? RefreshToken { get; set; }
    }
}
