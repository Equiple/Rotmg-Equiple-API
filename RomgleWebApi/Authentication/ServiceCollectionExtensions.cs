using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RotmgleWebApi.Authentication
{
    public static class ServiceCollectionExtensions
    {
        public static AuthenticationValidatorsBuilder<TIdentityProvider> AddAuthenticationValidators<TIdentityProvider>(
            this IServiceCollection services)
            where TIdentityProvider : struct, Enum
        {
            services.RemoveAll<IAuthenticationValidator<TIdentityProvider>>();
            AuthenticationValidatorsBuilder<TIdentityProvider> builder = new(services);
            return builder;
        }

        public static DeviceIdProvidersBuilder AddDeviceIdProviders(
            this IServiceCollection services,
            string defaultDeviceId)
        {
            services.RemoveAll<IDeviceIdProvider>();
            services.AddSingleton<IDeviceIdProviderCollection, DeviceIdProviderCollection>(
                s => new DeviceIdProviderCollection(
                    s.GetServices<IDeviceIdProvider>(),
                    defaultDeviceId));
            DeviceIdProvidersBuilder builder = new(services);
            return builder;
        }
    }
}
