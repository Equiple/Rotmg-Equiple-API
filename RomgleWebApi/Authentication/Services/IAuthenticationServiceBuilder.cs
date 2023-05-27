namespace RotmgleWebApi.Authentication
{
    public interface IAuthenticationServiceBuilder
    {
        IAuthenticationServiceBuilder AddValidator<T>() where T : class, IAuthenticationValidator;
    }
}
