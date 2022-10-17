namespace RomgleWebApi.Data.Models.Auth
{
    public class Device
    {
        public string Id { get; set; }

        public string PersonalKey { get; set; }

        public string PersonalKeyEncoding { get; set; }

        public List<RefreshToken> RefreshTokens { get; set; }
    }
}
