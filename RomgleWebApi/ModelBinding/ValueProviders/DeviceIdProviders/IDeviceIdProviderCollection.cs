namespace RotmgleWebApi.ModelBinding
{
    public interface IDeviceIdProviderCollection
    {
        IEnumerable<IDeviceIdProvider> Providers { get; }

        string GetFirstDefinedOrDefaultDeviceId(HttpContext httpContext);
    }
}
