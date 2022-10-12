using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomgleWebApi.Data.Models;
using RomgleWebApi.ModelBinding.Attributes;
using RomgleWebApi.Services;

namespace RomgleWebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/")]
    public class GameController : ControllerBase
    {
        private readonly IItemsService _itemsService;
        private readonly IPlayersService _playersService;
        private readonly IDailiesService _dailiesService;
        private readonly IGameService _gameService;
        private readonly ILogger<GameController> _logger;

        public GameController(
            ILogger<GameController> logger,
            IItemsService itemsService,
            IPlayersService playersService,
            IDailiesService dailiesService,
            IGameService gameService)
        {
            _logger = logger;
            _itemsService = itemsService;
            _playersService = playersService;
            _dailiesService = dailiesService;
            _gameService = gameService;
        }

        [AllowAnonymous]
        [HttpGet("FindAll")]
        public async Task<IReadOnlyList<Item>> FindAll(string searchInput, bool reskinsExcluded)
        {
            IReadOnlyList<Item> items = await _itemsService.FindAllAsync(searchInput, reskinsExcluded);
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
            bool result = await _playersService.WasDailyAttemptedAsync(playerId);
            return result;
        }

        [HttpGet("GetTries")]
        public async Task<int> GetTries([UserId] string playerId)
        {
            int tries = await _gameService.GetTriesAsync(playerId);
            return tries;
        }

        [HttpGet("GetGuesses")]
        public async Task<IReadOnlyList<Item>> GetGuesses([UserId] string playerId)
        {
            IReadOnlyList<Item> guesses = await _gameService.GetGuessesAsync(playerId);
            return guesses;
        }

        [HttpGet("GetActiveGameOptions")]
        public async Task<GameOptions?> GetActiveGameOptions([UserId] string playerId)
        {
            GameOptions? options = await _gameService.GetActiveGameOptionsAsync(playerId);
            return options;
        }

        [HttpGet("GetTargetItemName")]
        public async Task<string> GetTargetItemAsync([UserId] string playerId)
        {
            string item = await _gameService.GetTargetItemNameAsync(playerId);
            return item;
        }

        [HttpGet("GetGuess")]
        public async Task<Item?> GetGuess(string itemId)
        {
            Item? guess = await _itemsService.GetAsync(itemId);
            return guess;
        }

        [HttpGet("GetHints")]
        public async Task<Hints> GetHints([UserId] string playerId, string itemId)
        {
            Hints hints = await _gameService.GetHintsAsync(playerId, itemId);
            return hints;
        }

        [HttpGet("GetAllHints")]
        public async Task<IReadOnlyList<Hints>> GetHints([UserId] string playerId)
        {
            IReadOnlyList<Hints> allHints = await _gameService.GetHintsAsync(playerId);
            return allHints;
        }

        [HttpPost("CloseTheGame")]
        public async Task CloseTheGame([UserId] string playerId)
        {
            await _gameService.CloseTheGameAsync(playerId);
        }

        [HttpGet("GetCurrentStreak")]
        public async Task<int?> GetCurrentStreak([UserId] string playerId)
        {
            int? streak = await _gameService.GetCurrentStreakAsync(playerId);
            return streak;
        }
    }
}
