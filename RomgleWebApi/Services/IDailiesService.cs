using RomgleWebApi.Data.Models;

namespace RomgleWebApi.Services
{
    public interface IDailiesService
    {
        Task<Daily> GetAsync();
    }
}

