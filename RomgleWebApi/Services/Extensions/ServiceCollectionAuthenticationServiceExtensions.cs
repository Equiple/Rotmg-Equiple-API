using RomgleWebApi.Data.Settings;
using RomgleWebApi.IdentityValidators;
using RomgleWebApi.Services.Implementations;

namespace RomgleWebApi.Services.Extensions
{
    public static class ServiceCollectionAuthenticationServiceExtensions
    {
        public static IServiceCollection AddAuthenticationService(
            this IServiceCollection services,
            params IAuthenticationValidator[] authenticationValidators)
        {
            services.Configure<AuthenticationServiceSettings>(settings =>
            {
                settings.AuthenticationValidators.AddRange(authenticationValidators);
            });
            return services.AddSingleton<IAuthenticationService, AuthenticationService>();
        }
    }
}
