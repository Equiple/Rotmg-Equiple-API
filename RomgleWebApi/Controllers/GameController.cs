using Microsoft.AspNetCore.Mvc;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Services;

namespace RomgleWebApi.Controllers
{
    [ApiController]
    [Route("/")]
    public class GameController : ControllerBase
    {
        private readonly ItemsService _itemsService;
        private readonly PlayersService _playersService;
        private readonly DailiesService _dailiesService;
        private readonly GameService _gameService;
        private readonly ILogger<GameController> _logger;

        public GameController(ILogger<GameController> logger,
            ItemsService itemsService, PlayersService playersService, DailiesService dailiesService, GameService gameService)
        {
            _logger = logger;
            _itemsService = itemsService;
            _playersService = playersService;
            _dailiesService = dailiesService;
            _gameService = gameService;
        }

        [HttpGet("FindAll")]
        public async Task<IEnumerable<Item>> FindAll(string searchInput, bool reskinsExcluded) => 
            await _itemsService.FindAllAsync(searchInput, reskinsExcluded); 

        [HttpPost("CheckGuess")]
        public async Task<GuessResult> CheckGuess(string itemId, string playerId, Gamemode mode, bool reskinsExcluded) =>
            await _gameService.CheckGuessAsync(itemId, playerId, mode, reskinsExcluded);

        [HttpGet("WasDailyAttempted")]
        public async Task<bool> WasDailyAttempted(string playerId) =>
            await _playersService.WasDailyAttemptedAsync(playerId);

        [HttpGet("GetCurrentStreak")]
        public async Task<int> GetCurrentStreak(string playerId, Gamemode mode)
        {
            return await _playersService.GetCurrentStreakAsync(playerId, mode);
        }

        [HttpGet("GetBestStreak")]
        public async Task<int> GetBestStreak(string playerId, Gamemode mode)
        {
            return await _playersService.GetBestStreakAsync(playerId, mode);
        }

        [HttpPut("CreateNewPlayer")]
        public async Task CreateNewPlayer(string name, string password, string email) =>
            await _playersService.CreateNewPlayerAsync(name, password, email);

        [HttpGet("GetTries")]
        public async Task<int> GetTries(string playerId) =>
            await _gameService.GetTriesAsync(playerId);

        [HttpGet("GetGuesses")]
        public async Task<List<Item>> GetGuesses(string playerId) =>
            await _gameService.GetGuessesAsync(playerId);

        [HttpGet("GetActiveGameOptions")]
        public async Task<GameOptions?> GetActiveGameOptions(string playerId) =>
            await _gameService.GetActiveGameOptionsAsync(playerId);

        [HttpGet("GetTargetItemName")]
        public async Task<string> GetTargetItemAsync(string playerId)=>
            await _gameService.GetTargetItemNameAsync(playerId);

        [HttpGet("GetGuess")]
        public async Task<Item?> GetGuess(string itemId) =>
            await _itemsService.GetAsync(itemId);

        [HttpGet("GetHints")]
        public async Task<Hints> GetHints(string playerId, string itemId) =>
            await _gameService.GetHintsAsync(playerId, itemId);

        [HttpGet("GetAllHints")]
        public async Task<List<Hints>> GetHints(string playerId) =>
            await _gameService.GetHintsAsync(playerId);

        [HttpPost("CloseTheGame")]
        public async Task CloseTheGame(string playerId) =>
            await _gameService.CloseTheGameAsync(playerId);

        [HttpGet("GetTargetItemImage")]
        public async Task<string> GetTargetItemImage(string playerId)
        {
            return await _gameService.GetTargetItemImage(playerId);
        }

        [HttpGet("GetDailyLeaderboard")]
        public async Task<List<PlayerProfile>> GetDailyLeaderboard()
        {
            return await _playersService.GetDailyLeaderboardAsync();
        }

        [HttpGet("GetNormalLeaderboard")]
        public async Task<List<PlayerProfile>> GetNormalLeaderboard()
        {
            return await _playersService.GetNormalLeaderboardAsync();
        }

        [HttpGet("GetPlayerLeaderboardPlacement")]
        public async Task<int> GetPlayerLeaderboardPlacement(string playerId, Gamemode mode)
        {
            return await _gameService.GetPlayerLeaderboardPlacementAsync(playerId, mode);
        }
    }
}
