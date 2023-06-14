namespace RotmgleWebApi.AuthenticationImplementation
{
    public class Session
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public DateAndTime AccessExpiresAt { get; set; }

        public DateAndTime ExpiresAt { get; set; }

        public string UserId { get; set; }

        public string DeviceId { get; set; }

        public Dictionary<string, string> Payload { get; set; }
    }
}
