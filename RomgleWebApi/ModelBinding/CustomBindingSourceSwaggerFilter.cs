using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RotmgleWebApi.ModelBinding
{
    public class CustomBindingSourceSwaggerFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            foreach (ApiParameterDescription paramDesc in context.ApiDescription.ParameterDescriptions)
            {
                if (paramDesc.Source is CustomBindingSource customSource)
                {
                    int paramIndex = operation.Parameters.FirstIndex(p => p.Name == paramDesc.Name);
                    OpenApiParameter param = operation.Parameters[paramIndex];
                    if (customSource.ParameterFrom.HasValue)
                    {
                        param.In = customSource.ParameterFrom.Value;
                    }
                    else
                    {
                        operation.Parameters.RemoveAt(paramIndex);
                    }
                }
            }
        }
    }
}
