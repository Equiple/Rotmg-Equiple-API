namespace RotmgleWebApi.ModelBinding
{
    public interface IDeviceIdProviderCollectionBuilder
    {
        IDeviceIdProviderCollectionBuilder Add<T>() where T : class, IDeviceIdProvider;
    }
}
