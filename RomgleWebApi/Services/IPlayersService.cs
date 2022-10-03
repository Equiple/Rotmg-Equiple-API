using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.Services
{
    public interface IPlayersService
    {
        Task<Player> GetAsync(string id);

        Task<Player?> GetByIdentityAsync(Identity identity);

        Task<Player?> GetByRefreshTokenAsync(string refreshToken);

        Task UpdateAsync(Player updatedPlayer);

        Task<bool> WasDailyAttemptedAsync(string id);

        Task<Player> CreateNewPlayerAsync(Identity identity);
    }
}
