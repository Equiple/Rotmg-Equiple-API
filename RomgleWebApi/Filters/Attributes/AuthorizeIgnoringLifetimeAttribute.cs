using Microsoft.AspNetCore.Mvc;

namespace RomgleWebApi.Filters.Attributes
{
    public class AuthorizeIgnoringLifetimeAttribute : TypeFilterAttribute
    {
        public AuthorizeIgnoringLifetimeAttribute() : base(typeof(AuthorizeIgnoringLifetimeFilter))
        {
        }
    }
}
