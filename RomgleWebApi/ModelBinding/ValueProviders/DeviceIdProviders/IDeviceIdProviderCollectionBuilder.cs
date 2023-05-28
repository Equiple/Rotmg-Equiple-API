namespace RotmgleWebApi.ModelBinding
{
    public interface IDeviceIdProviderCollectionBuilder
    {
        IDeviceIdProviderCollectionBuilder Add<T>() where T : class, IDeviceIdProvider;

        IDeviceIdProviderCollectionBuilder Add<T>(T provider) where T : class, IDeviceIdProvider;
    }
}
