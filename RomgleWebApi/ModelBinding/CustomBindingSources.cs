using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RotmgleWebApi.ModelBinding
{
    public static class CustomBindingSources
    {
        public static readonly BindingSource User = new BindingSource(
            id: nameof(User),
            displayName: $"{nameof(BindingSource)}_{nameof(User)}",
            isGreedy: false,
            isFromRequest: false);
    }
}
