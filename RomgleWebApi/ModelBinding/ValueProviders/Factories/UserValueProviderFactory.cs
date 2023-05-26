using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RotmgleWebApi.ModelBinding
{
    public class UserValueProviderFactory : IValueProviderFactory
    {
        public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
        {
            UserValueProvider provider = new(context.ActionContext.HttpContext.User);
            context.ValueProviders.Add(provider);
            return Task.CompletedTask;
        }
    }
}
