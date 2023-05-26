using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;

namespace RotmgleWebApi.ModelBinding
{
    public class UserValueProvider : BindingSourceValueProvider
    {
        private readonly ClaimsPrincipal _user;

        public UserValueProvider(ClaimsPrincipal user) : base(CustomBindingSources.User)
        {
            _user = user;
        }

        public override bool ContainsPrefix(string prefix)
        {
            prefix = prefix.ToLower();
            bool hasClaim = _user.HasClaim(claim => claim.Type.ToLower() == prefix);
            return hasClaim;
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
