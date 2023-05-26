namespace RotmgleWebApi.Authentication
{
    public class RefreshToken
    {
        public string Token { get; set; }

        public DateAndTime Expires { get; set; }

        public DateAndTime? Revoked { get; set; }
    }
}
