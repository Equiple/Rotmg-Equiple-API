﻿using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RotmgleWebApi.ModelBinding
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromUserAttribute : Attribute, IBindingSourceMetadata, IModelNameProvider
    {
        public FromUserAttribute(string claimType)
        {
            Name = claimType;
        }

        public BindingSource? BindingSource => CustomBindingSource.User;

        public string? Name { get; }
    }
}
