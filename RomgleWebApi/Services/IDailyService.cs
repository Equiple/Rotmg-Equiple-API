using RomgleWebApi.Data.Models;

namespace RomgleWebApi.Services
{
    public interface IDailyService
    {
        Task<Daily> GetAsync();
    }
}

