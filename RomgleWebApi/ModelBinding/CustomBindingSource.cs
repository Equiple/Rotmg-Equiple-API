using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;

namespace RotmgleWebApi.ModelBinding
{
    public class CustomBindingSource : BindingSource
    {
        public static readonly CustomBindingSource User = new(
            id: "User",
            displayName: "BindingSource_User",
            isGreedy: false,
            isFromRequest: true);

        public CustomBindingSource(
            string id,
            string displayName,
            bool isGreedy,
            bool isFromRequest,
            ParameterLocation? parameterFrom = null)
            : base(id, displayName, isGreedy, isFromRequest)
        {
            ParameterFrom = parameterFrom;
        }

        public ParameterLocation? ParameterFrom { get; }
    }
}
