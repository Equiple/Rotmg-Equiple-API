using RomgleWebApi.Data.Models;

namespace RomgleWebApi.Services
{
    public interface IDailiesService
    {
        Task<Daily> GetAsync();

        Task<Daily> GetAsync(string id);

        Task<Daily?> GetAsync(DateTime timestamp);

        Task CreateAsync(string itemId);

        Task<Daily> GetDailyItem();

        Task<bool> CheckDailyItem(string itemId);
    }
}

