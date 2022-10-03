using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RomgleWebApi.ModelBinding.ValueProviders.Factories
{
    public class CookieValueProviderFactory : IValueProviderFactory
    {
        public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
        {
            CookieValueProvider provider = new CookieValueProvider(context.ActionContext.HttpContext.Request.Cookies);
            context.ValueProviders.Add(provider);
            return Task.CompletedTask;
        }
    }
}
