using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;

namespace RotmgleWebApi.ModelBinding
{
    public class UserValueProvider : BindingSourceValueProvider
    {
        private readonly ClaimsPrincipal _user;

        public UserValueProvider(ClaimsPrincipal user) : base(CustomBindingSource.User)
        {
            _user = user;
        }

        public override bool ContainsPrefix(string prefix)
        {
            return true;
        }

        public override ValueProviderResult GetValue(string key)
        {
            string claimValue = _user.FindFirstValue(key);
            if (string.IsNullOrWhiteSpace(claimValue))
            {
                return ValueProviderResult.None;
            }

            ValueProviderResult result = new(claimValue);
            return result;
        }
    }
}
