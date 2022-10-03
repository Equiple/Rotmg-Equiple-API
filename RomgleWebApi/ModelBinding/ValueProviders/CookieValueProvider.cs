using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RomgleWebApi.ModelBinding.ValueProviders
{
    public class CookieValueProvider : BindingSourceValueProvider
    {
        private IRequestCookieCollection _cookies;

        public CookieValueProvider(IRequestCookieCollection cookies) : base(CustomBindingSources.Cookie)
        {
            _cookies = cookies;
        }

        public override bool ContainsPrefix(string prefix)
        {
            bool hasCookie = _cookies.ContainsKey(prefix);
            return hasCookie;
        }

        public override ValueProviderResult GetValue(string key)
        {
            if (!_cookies.TryGetValue(key, out string? cookieValue) ||
                string.IsNullOrWhiteSpace(cookieValue))
            {
                return ValueProviderResult.None;
            }

            ValueProviderResult result = new ValueProviderResult(cookieValue);
            return result;
        }
    }
}
