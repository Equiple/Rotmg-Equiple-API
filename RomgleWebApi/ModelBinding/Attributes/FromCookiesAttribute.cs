using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RomgleWebApi.ModelBinding.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromCookiesAttribute : Attribute, IBindingSourceMetadata, IModelNameProvider
    {
        public FromCookiesAttribute(string cookieKey)
        {
            Name = cookieKey;
        }

        public BindingSource? BindingSource => CustomBindingSources.Cookie;

        public string? Name { get; }
    }
}
