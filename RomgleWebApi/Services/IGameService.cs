using RomgleWebApi.Data.Models;

namespace RomgleWebApi.Services
{
    public interface IGameService
    {
        Task<GuessResult> CheckGuessAsync(string playerId, string guessId, Gamemode mode, bool reskinsExcluded);

        Task<int> GetTriesAsync(string playerId);

        Task<IReadOnlyList<Item>> GetGuessesAsync(string playerId);

        Task<IReadOnlyList<Hints>> GetHintsAsync(string playerId);

        Task<Hints> GetHintsAsync(string playerId, string guessId);

        Task<string> GetTargetItemNameAsync(string playerId);

        Task<GameOptions?> GetActiveGameOptionsAsync(string playerId);

        Task<int?> GetCurrentStreakAsync(string playerId);

        Task CloseTheGameAsync(string playerId);
    }
}
