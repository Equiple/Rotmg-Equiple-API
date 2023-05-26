namespace RotmgleWebApi.Authentication
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthenticationService(
            this IServiceCollection services,
            params IAuthenticationValidator[] authenticationValidators)
        {
            services.Configure<AuthenticationServiceSettings>(settings =>
            {
                settings.AuthenticationValidators.Clear();
                settings.AuthenticationValidators.AddRange(authenticationValidators);
            });
            return services.AddSingleton<IAuthenticationService, AuthenticationService>();
        }
    }
}
