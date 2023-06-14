namespace RotmgleWebApi.Authentication
{
    public record class Session(
        string AccessToken,
        string RefreshToken,
        DateTime AccessExpiresAt,
        DateTime ExpiresAt,
        string UserId,
        string DeviceId,
        IReadOnlyDictionary<string, string> Payload)
    {
        public bool IsAccessExpired => DateTime.UtcNow > AccessExpiresAt;

        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    };
}
