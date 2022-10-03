namespace RomgleWebApi.Data.Models.Auth
{
    public interface IAuthenticationResponse : IIsAuthenticatedResponse
    {
        string? AccessToken { get; }

        string? RefreshToken { get; }
    }
}
