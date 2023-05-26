namespace RotmgleWebApi.Authentication
{
    public class TokenAuthorizationSettings
    {
        public string? Issuer { get; set; }

        public string? Audience { get; set; }

        public int AccessTokenLifetimeMinutes { get; set; }

        public int RefreshTokenByteLength { get; set; }

        public int RefreshTokenLifetimeDays { get; set; }

        public string SecretKey { get; set; }
    }
}
