namespace RotmgleWebApi.Authentication
{
    public class UserAgentDeviceIdProvider : IDeviceIdProvider
    {
        public string? GetDeviceId(HttpContext httpContext)
        {
            string userAgent = httpContext.Request.Headers.UserAgent;
            if (string.IsNullOrEmpty(userAgent))
            {
                return null;
            }
            return userAgent;
        }
    }
}
