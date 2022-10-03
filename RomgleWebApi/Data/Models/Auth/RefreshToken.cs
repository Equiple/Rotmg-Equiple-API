namespace RomgleWebApi.Data.Models.Auth
{
    public class RefreshToken
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public DateTime? Revoked { get; set; }
    }
}
