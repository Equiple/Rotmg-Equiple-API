namespace RotmgleWebApi.ModelBinding
{
    public interface IDeviceIdProvider
    {
        string? GetDeviceId(HttpContext httpContext);
    }
}
