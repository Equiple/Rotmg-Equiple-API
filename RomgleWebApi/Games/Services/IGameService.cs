using RotmgleWebApi.Items;

namespace RotmgleWebApi.Games
{
    public interface IGameService
    {
        Task<GuessResult> CheckGuessAsync(string playerId, string guessId, Gamemode mode, bool reskinsExcluded);

        Task<int> GetTriesAsync(string playerId);

        Task<IEnumerable<Item>> GetGuessesAsync(string playerId);

        Task<IEnumerable<Hints>> GetHintsAsync(string playerId);

        Task<Hints> GetHintsAsync(string playerId, string guessId);

        Task<Item> GetTargetItemAsync(string playerId);

        Task<GameOptions?> GetActiveGameOptionsAsync(string playerId);

        Task CloseGameAsync(string playerId);
    }
}
