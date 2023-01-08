using Hangfire;
using RomgleWebApi.Data;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.Services
{
    public interface IPlayerService
    {
        Task<Player> GetAsync(string playerId);

        Task InvalidateExpiredDailyGamesAsync();

        Task RemoveInactiveGuestAccountsAsync();

        Task<PlayerByIdentity?> GetByIdentityAsync(Identity identity);

        Task<NewPlayer> CreateNewAsync(Identity identity, string? name = null);

        Task<Device> CreateNewDeviceAsync(string playerId);

        Task UpdateAsync(Player updatedPlayer);

        Task RefreshPersonalKeyAsync(string playerId, string deviceId);

        Task<bool> WasDailyAttemptedAsync(string playerId);

        Task<int> GetBestStreakAsync(string playerId, Gamemode mode);

        Task<int> GetCurrentStreakAsync(string playerId, Gamemode mode);

        Task<DetailedGameStatistic> GetPlayerStatsAsync(string playerId, Gamemode mode);

        Task<PlayerProfile> GetPlayerProfileAsync(string playerId);

        Task<IReadOnlyList<PlayerProfile>> GetDailyLeaderboardAsync();

        Task<IReadOnlyList<PlayerProfile>> GetNormalLeaderboardAsync();

        Task<int> GetPlayerLeaderboardPlacementAsync(string playerId, Gamemode mode);

        Task UpdatePlayerScoreAsync(Player player, GameResult result);
    }
}
