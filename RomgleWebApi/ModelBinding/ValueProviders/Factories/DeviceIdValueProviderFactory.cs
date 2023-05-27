using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RotmgleWebApi.ModelBinding
{
    public class DeviceIdValueProviderFactory : IValueProviderFactory
    {
        public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
        {
            DeviceIdValueProvider provider = new(context.ActionContext.HttpContext);
            context.ValueProviders.Add(provider);
            return Task.CompletedTask;
        }
    }
}
