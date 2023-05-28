namespace RotmgleWebApi.Authentication
{
    public interface IAuthenticationServiceBuilder
    {
        IAuthenticationServiceBuilder AddValidator<T>() where T : class, IAuthenticationValidator;

        IAuthenticationServiceBuilder AddValidator<T>(T validator) where T : class, IAuthenticationValidator;
    }
}
