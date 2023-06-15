using RotmgleWebApi.AuthenticationImplementation;
using RotmgleWebApi.Games;

namespace RotmgleWebApi.Players
{
    public interface IPlayerService
    {
        Task<Player> GetAsync(string playerId);

        Task<Player?> GetOrDefaultAsync(string playerId);

        Task<Player?> GetByIdentityAsync(Identity identity);

        Task<Player> CreateNewAsync(string? name, Identity? identity);

        Task UpdateAsync(Player player);

        Task<bool> WasDailyAttemptedAsync(string playerId);

        Task<int> GetBestStreakAsync(string playerId, Gamemode mode);

        Task<int> GetCurrentStreakAsync(string playerId, Gamemode mode);

        Task<GameStatisticDetailed> GetPlayerStatsAsync(string playerId, Gamemode mode);

        Task<PlayerProfile> GetPlayerProfileAsync(string playerId);

        Task<IEnumerable<PlayerProfile>> GetDailyLeaderboardAsync();

        Task<IEnumerable<PlayerProfile>> GetNormalLeaderboardAsync();

        Task<int> GetPlayerLeaderboardPlacementAsync(string playerId, Gamemode mode);

        Task UpdatePlayerScoreAsync(Player player, GameResult result);

        Task InvalidateExpiredDailyGamesAsync();

        Task RemoveInactiveGuestAccountsAsync();
    }
}
