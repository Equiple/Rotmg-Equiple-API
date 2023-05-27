using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RotmgleWebApi.ModelBinding
{
    public class DeviceIdValueProvider : BindingSourceValueProvider
    {
        private readonly HttpContext _httpContext;

        public DeviceIdValueProvider(HttpContext httpContext) : base(CustomBindingSource.DeviceId)
        {
            _httpContext = httpContext;
        }

        public override bool ContainsPrefix(string prefix)
        {
            return true;
        }

        public override ValueProviderResult GetValue(string key)
        {
            IDeviceIdProviderCollection? deviceIdProviderCollection = _httpContext.RequestServices
                .GetService<IDeviceIdProviderCollection>();
            if (deviceIdProviderCollection == null)
            {
                throw new Exception("Couldn't get device id");
            }
            string deviceId = deviceIdProviderCollection.GetFirstDefinedOrDefaultDeviceId(_httpContext);
            return new ValueProviderResult(deviceId);
        }
    }
}
