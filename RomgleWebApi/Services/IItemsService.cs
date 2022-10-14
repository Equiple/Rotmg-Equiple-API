using RomgleWebApi.Data.Models;

namespace RomgleWebApi.Services
{
    public interface IItemsService
    {
        Task<Item> GetAsync(string itemId);

        Task<IReadOnlyList<Item>> FindAllAsync(string searchInput, bool reskinsExcluded);

        Task<Item> GetRandomItemAsync(bool reskinsExcluded);
    }
}
