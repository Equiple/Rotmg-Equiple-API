namespace RomgleWebApi.Data.Settings
{
    public class TokenAuthorizationSettings
    {
        public string? Issuer { get; set; }

        public string? Audience { get; set; }

        public int AccessTokenLifetimeMinutes { get; set; }

        public int RefreshTokenLifetimeDays { get; set; }

        public string SecretKey { get; set; }

        public string SecretKeyEncoding { get; set; }

        public bool ValidateIssuer => !string.IsNullOrWhiteSpace(Issuer);

        public bool ValidateAudience => !string.IsNullOrWhiteSpace(Audience);
    }
}
