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
        private readonly IItemService _itemsService;
        private readonly IPlayerService _playersService;
        private readonly IDailyService _dailiesService;
        private readonly IGameService _gameService;
        private readonly ILogger<GameController> _logger;

        public GameController(
            ILogger<GameController> logger,
            IItemService itemsService,
            IPlayerService playersService,
            IDailyService dailiesService,
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

        [HttpGet("GetCurrentStreak")]
        public async Task<int> GetCurrentStreak([UserId] string playerId, Gamemode mode)
        {
            return await _playersService.GetCurrentStreakAsync(playerId, mode);
        }

        [HttpGet("GetBestStreak")]
        public async Task<int> GetBestStreak([UserId] string playerId, Gamemode mode)
        {
            return await _playersService.GetBestStreakAsync(playerId, mode);
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

        [HttpGet("GetGuess")]
        public async Task<Item> GetGuess(string itemId)
        {
            Item guess = await _itemsService.GetAsync(itemId);
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
            await _gameService.CloseGameAsync(playerId);
        }

        [HttpGet("GetTargetItem")]
        public async Task<Item> GetTargetItem([UserId] string playerId)
        {
            return await _gameService.GetTargetItemAsync(playerId);
        }

        [HttpGet("GetDailyLeaderboard")]
        public async Task<IReadOnlyList<PlayerProfile>> GetDailyLeaderboard()
        {
            return await _playersService.GetDailyLeaderboardAsync();
        }

        [HttpGet("GetNormalLeaderboard")]
        public async Task<IReadOnlyList<PlayerProfile>> GetNormalLeaderboard()
        {
            return await _playersService.GetNormalLeaderboardAsync();
        }

        [HttpGet("GetPlayerLeaderboardPlacement")]
        public async Task<int> GetPlayerLeaderboardPlacement([UserId] string playerId, Gamemode mode)
        {
            return await _playersService.GetPlayerLeaderboardPlacementAsync(playerId, mode);
        }
    }
}
