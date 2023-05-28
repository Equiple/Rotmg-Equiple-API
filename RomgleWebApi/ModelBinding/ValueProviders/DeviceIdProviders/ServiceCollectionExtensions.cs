using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RotmgleWebApi.ModelBinding
{
    public static class ServiceCollectionExtensions
    {
        public static IDeviceIdProviderCollectionBuilder AddDeviceIdProviders(
            this IServiceCollection services,
            string defaultDeviceId)
        {
            services.RemoveAll<IDeviceIdProvider>();
            services.AddSingleton<IDeviceIdProviderCollection, DeviceIdProviderCollection>(
                s => new DeviceIdProviderCollection(
                    s.GetServices<IDeviceIdProvider>(),
                    defaultDeviceId));
            DeviceIdProviderCollectionBuilder builder = new(services);
            return builder;
        }

        private class DeviceIdProviderCollection : IDeviceIdProviderCollection
        {
            private readonly string _defaultDeviceId;

            public DeviceIdProviderCollection(
                IEnumerable<IDeviceIdProvider> providers,
                string defaultDeviceId)
            {
                Providers = providers;
                _defaultDeviceId = defaultDeviceId;
            }

            public IEnumerable<IDeviceIdProvider> Providers { get; }

            public string GetFirstDefinedOrDefaultDeviceId(HttpContext httpContext)
            {
                foreach (IDeviceIdProvider device in Providers)
                {
                    string? deviceId = device.GetDeviceId(httpContext);
                    if (deviceId != null)
                    {
                        return deviceId;
                    }
                }
                return _defaultDeviceId;
            }
        }

        private class DeviceIdProviderCollectionBuilder : IDeviceIdProviderCollectionBuilder
        {
            private readonly IServiceCollection _services;

            public DeviceIdProviderCollectionBuilder(IServiceCollection services)
            {
                _services = services;
            }

            public IDeviceIdProviderCollectionBuilder Add<T>() where T : class, IDeviceIdProvider
            {
                _services.AddSingleton<IDeviceIdProvider, T>();
                return this;
            }

            public IDeviceIdProviderCollectionBuilder Add<T>(T provider) where T : class, IDeviceIdProvider
            {
                _services.AddSingleton(provider);
                return this;
            }
        }
    }
}
