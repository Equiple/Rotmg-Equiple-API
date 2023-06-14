namespace RotmgleWebApi.Authentication
{
    public class DeviceIdProvidersBuilder
    {
        private readonly IServiceCollection _services;

        public DeviceIdProvidersBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public DeviceIdProvidersBuilder AddDeviceIdProvider<T>()
            where T : class, IDeviceIdProvider
        {
            _services.AddSingleton<IDeviceIdProvider, T>();
            return this;
        }

        public DeviceIdProvidersBuilder AddDeviceIdProvider<T>(T provider)
            where T : class, IDeviceIdProvider
        {
            _services.AddSingleton(provider);
            return this;
        }

        public DeviceIdProvidersBuilder AddUserAgentDeviceIdProvider()
        {
            _services.AddSingleton<IDeviceIdProvider, UserAgentDeviceIdProvider>();
            return this;
        }
    }
}
