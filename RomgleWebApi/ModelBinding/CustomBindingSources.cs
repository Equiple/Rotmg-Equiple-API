using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RomgleWebApi.ModelBinding
{
    public static class CustomBindingSources
    {
        public static readonly BindingSource User = new BindingSource(
            id: nameof(User),
            displayName: $"{nameof(BindingSource)}_{nameof(User)}",
            isGreedy: false,
            isFromRequest: false);

        public static readonly BindingSource Cookie = new BindingSource(
            id: nameof(Cookie),
            displayName: $"{nameof(BindingSource)}_{nameof(User)}",
            isGreedy: false,
            isFromRequest: false);
    }
}
