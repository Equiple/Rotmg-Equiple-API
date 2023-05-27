using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RotmgleWebApi.Games;
using RotmgleWebApi.Items;
using RotmgleWebApi.ModelBinding;
using RotmgleWebApi.Players;

namespace RotmgleWebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/")]
    public class GameController : ControllerBase
    {
        private readonly IItemService _itemService;
        private readonly IPlayerService _playerService;
        private readonly IGameService _gameService;
        private readonly ILogger<GameController> _logger;

        public GameController(
            IItemService itemService,
            IPlayerService playerService,
            IGameService gameService,
            ILogger<GameController> logger)
        {
            _itemService = itemService;
            _playerService = playerService;
            _gameService = gameService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet("FindAll")]
        public async Task<IEnumerable<Item>> FindAll(string searchInput, bool reskinsExcluded)
        {
            IEnumerable<Item> items = await _itemService.FindAllAsync(searchInput, reskinsExcluded);
            return items;
        }

        [HttpPost("CheckGuess")]
        public async Task<GuessResult> CheckGuess([UserId] string playerId, string itemId, Gamemode mode, bool reskinsExcluded)
        {
            GuessResult result = await _gameService.CheckGuessAsync(playerId, itemId, mode, reskinsExcluded);
            return result;
        }

        [HttpGet("WasDailyAttempted")]
        public async Task<bool> WasDailyAttempted([UserId] string playerId)
        {
            bool result = await _playerService.WasDailyAttemptedAsync(playerId);
            return result;
        }

        [HttpGet("GetCurrentStreak")]
        public async Task<int> GetCurrentStreak([UserId] string playerId, Gamemode mode)
        {
            return await _playerService.GetCurrentStreakAsync(playerId, mode);
        }

        [HttpGet("GetBestStreak")]
        public async Task<int> GetBestStreak([UserId] string playerId, Gamemode mode)
        {
            return await _playerService.GetBestStreakAsync(playerId, mode);
        }

        [HttpGet("GetTries")]
        public async Task<int> GetTries([UserId] string playerId)
        {
            int tries = await _gameService.GetTriesAsync(playerId);
            return tries;
        }

        [HttpGet("GetGuesses")]
        public async Task<IEnumerable<Item>> GetGuesses([UserId] string playerId)
        {
            IEnumerable<Item> guesses = await _gameService.GetGuessesAsync(playerId);
            return guesses;
        }

        [HttpGet("GetActiveGameOptions")]
        public async Task<GameOptions?> GetActiveGameOptions([UserId] string playerId)
        {
            GameOptions? options = await _gameService.GetActiveGameOptionsAsync(playerId);
            return options;
        }

        [HttpGet("GetGuess")]
        public async Task<Item> GetGuess(string itemId)
        {
            Item guess = await _itemService.GetAsync(itemId);
            return guess;
        }

        [HttpGet("GetHints")]
        public async Task<Hints> GetHints([UserId] string playerId, string itemId)
        {
            Hints hints = await _gameService.GetHintsAsync(playerId, itemId);
            return hints;
        }

        [HttpGet("GetAllHints")]
        public async Task<IEnumerable<Hints>> GetHints([UserId] string playerId)
        {
            IEnumerable<Hints> allHints = await _gameService.GetHintsAsync(playerId);
            return allHints;
        }

        [HttpPost("CloseTheGame")]
        public async Task CloseTheGame([UserId] string playerId)
        {
            await _gameService.CloseGameAsync(playerId);
        }

        [HttpGet("GetTargetItem")]
        public async Task<Item> GetTargetItem([UserId] string playerId)
        {
            return await _gameService.GetTargetItemAsync(playerId);
        }

        [HttpGet("GetDailyLeaderboard")]
        public async Task<IEnumerable<PlayerProfile>> GetDailyLeaderboard()
        {
            return await _playerService.GetDailyLeaderboardAsync();
        }

        [HttpGet("GetNormalLeaderboard")]
        public async Task<IEnumerable<PlayerProfile>> GetNormalLeaderboard()
        {
            return await _playerService.GetNormalLeaderboardAsync();
        }

        [HttpGet("GetPlayerLeaderboardPlacement")]
        public async Task<int> GetPlayerLeaderboardPlacement([UserId] string playerId, Gamemode mode)
        {
            return await _playerService.GetPlayerLeaderboardPlacementAsync(playerId, mode);
        }
    }
}
