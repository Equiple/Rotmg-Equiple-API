namespace RotmgleWebApi.Authentication
{
    public interface IDeviceIdProvider
    {
        string? GetDeviceId(HttpContext httpContext);
    }
}
