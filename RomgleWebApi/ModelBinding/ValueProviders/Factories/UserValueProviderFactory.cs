using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RomgleWebApi.ModelBinding.ValueProviders.Factories
{
    public class UserValueProviderFactory : IValueProviderFactory
    {
        public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
        {
            UserValueProvider provider = new UserValueProvider(context.ActionContext.HttpContext.User);
            context.ValueProviders.Add(provider);
            return Task.CompletedTask;
        }
    }
}
