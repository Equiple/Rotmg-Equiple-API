namespace RomgleWebApi.Data.Models.Auth
{
    public class Identity
    {
        public IdentityProvider Provider { get; set; }

        public string Id { get; set; }

        public IdentityDetails Details { get; set; }
    }
}
