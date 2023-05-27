using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RotmgleWebApi.ModelBinding
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class DeviceIdAttribute : Attribute, IBindingSourceMetadata
    {
        public BindingSource? BindingSource => CustomBindingSource.DeviceId;
    }
}
