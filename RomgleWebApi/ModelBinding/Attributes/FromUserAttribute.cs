using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RomgleWebApi.ModelBinding.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromUserAttribute : Attribute, IBindingSourceMetadata, IModelNameProvider
    {
        public FromUserAttribute(string claimType)
        {
            Name = claimType;
        }

        public BindingSource? BindingSource => CustomBindingSources.User;

        public string? Name { get; }
    }
}
