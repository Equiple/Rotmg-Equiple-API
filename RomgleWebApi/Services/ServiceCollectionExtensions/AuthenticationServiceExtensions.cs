using RomgleWebApi.Data.Settings;
using RomgleWebApi.IdentityValidators;
using RomgleWebApi.Services.Implementations;

namespace RomgleWebApi.Services.ServiceCollectionExtensions
{
    public static class AuthenticationServiceExtensions
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
