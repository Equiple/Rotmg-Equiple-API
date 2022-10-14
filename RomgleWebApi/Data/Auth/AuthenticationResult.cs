using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.Data.Auth
{
    public readonly struct AuthenticationResult : IAuthenticationResponse
    {
        public static AuthenticationResult Success(string accessToken, string refreshToken)
        {
            return new AuthenticationResult(true, accessToken, refreshToken);
        }

        public static readonly AuthenticationResult Failure = new AuthenticationResult(false);

        private AuthenticationResult(
            bool isAuthenticated,
            string? accessToken = null,
            string? refreshToken = null)
        {
            IsAuthenticated = isAuthenticated;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }

        public bool IsAuthenticated { get; }

        public string? AccessToken { get; }

        public string? RefreshToken { get; }
    }
}
