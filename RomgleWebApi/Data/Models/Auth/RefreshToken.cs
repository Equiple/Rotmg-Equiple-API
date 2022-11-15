namespace RomgleWebApi.Data.Models.Auth
{
    public class RefreshToken
    {
        public string Id { get; set; }

        public string Token { get; set; }

        public string DeviceId { get; set; }

        public DateAndTime Expires { get; set; }

        public DateAndTime? Revoked { get; set; }
    }
}
