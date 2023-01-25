namespace RomgleWebApi.Data.Models
{
    public class Complaint
    {
        public string Id { get; set; }

        public string Fingerprint { get; set; }

        public string Email { get; set; }

        public DateAndTime Date { get; set; }

        public string Body { get; set; }
    }
}
