namespace RotmgleWebApi.Authentication
{
    public static class AuthenticationResponseUtils
    {
        public static AuthenticationResponse ToResponse(this AuthenticationResult result)
        {
            AuthenticationResponse response = new()
            {
                IsAuthenticated = true,
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken,
            };
            return response;
        }

        public static AuthenticationResponse ToResponse(this Result<AuthenticationResult> result)
        {
            if (result is Result<AuthenticationResult>.Ok ok) {
                return ok.ToResponse();
            }
            return new AuthenticationResponse
            {
                IsAuthenticated = false,
            };
        }
    }
}
