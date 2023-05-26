namespace RotmgleWebApi.Items
{
    public interface IItemService
    {
        Task<Item> GetAsync(string itemId);

        Task<IEnumerable<Item>> FindAllAsync(string searchInput, bool reskinsExcluded);

        Task<Item> GetRandomItemAsync(bool reskinsExcluded);
    }
}
