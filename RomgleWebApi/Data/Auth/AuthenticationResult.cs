using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.Data.Auth
{
    public readonly struct AuthenticationResult
    {
        public static AuthenticationResult Success(string accessToken, string refreshToken, string deviceId)
        {
            return new AuthenticationResult(true, accessToken, refreshToken, deviceId);
        }

        public static readonly AuthenticationResult Failure = new AuthenticationResult(false);

        private AuthenticationResult(
            bool isAuthenticated,
            string? accessToken = null,
            string? refreshToken = null,
            string? deviceId = null)
        {
            IsAuthenticated = isAuthenticated;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            DeviceId = deviceId;
        }

        public bool IsAuthenticated { get; }

        public string? AccessToken { get; }

        public string? RefreshToken { get; }

        public string? DeviceId { get; }

        public static implicit operator AuthenticationResponse(AuthenticationResult result)
        {
            return new AuthenticationResponse
            {
                IsAuthenticated = result.IsAuthenticated,
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken,
                DeviceId = result.DeviceId
            };
        }
    }
}
