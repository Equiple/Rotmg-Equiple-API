namespace RotmgleWebApi.Authentication
{
    public class DeviceIdProviderCollection : IDeviceIdProviderCollection
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
}
