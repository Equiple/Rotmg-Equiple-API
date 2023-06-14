namespace RotmgleWebApi.Authentication
{
    public class AuthenticationValidatorsBuilder<TIdentityProvider>
        where TIdentityProvider : struct, Enum
    {
        private readonly IServiceCollection _services;

        public AuthenticationValidatorsBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public AuthenticationValidatorsBuilder<TIdentityProvider> AddValidator<T>()
            where T : class, IAuthenticationValidator<TIdentityProvider>
        {
            _services.AddSingleton<IAuthenticationValidator<TIdentityProvider>, T>();
            return this;
        }

        public AuthenticationValidatorsBuilder<TIdentityProvider> AddValidator<T>(T validator)
            where T : class, IAuthenticationValidator<TIdentityProvider>
        {
            _services.AddSingleton(validator);
            return this;
        }
    }
}
