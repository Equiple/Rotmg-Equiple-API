using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.Services
{
    public interface IPlayersService
    {
        Task<Player> GetAsync(string playerId);

        Task<Player?> GetByIdentityAsync(Identity identity);

        Task<Player?> GetByRefreshTokenAsync(string refreshToken);

        Task<Player> CreateNewAsync(Identity identity);

        Task UpdateAsync(Player updatedPlayer);

        Task RefreshSecretKeyAsync(string playerId);

        Task<bool> WasDailyAttemptedAsync(string playerId);

        Task<int> GetBestStreakAsync(string playerId, Gamemode mode);

        Task<int> GetCurrentStreakAsync(string playerId, Gamemode mode);

        Task<DetailedGameStatistic> GetPlayerStatsAsync(string playerId, Gamemode mode);

        Task<PlayerProfile> GetPlayerProfileAsync(string playerId);

        Task<IReadOnlyList<PlayerProfile>> GetDailyLeaderboardAsync();

        Task<IReadOnlyList<PlayerProfile>> GetNormalLeaderboardAsync();

        Task<int> GetPlayerLeaderboardPlacementAsync(string playerId, Gamemode mode);
    }
}
