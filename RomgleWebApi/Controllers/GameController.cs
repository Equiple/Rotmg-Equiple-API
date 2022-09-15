using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Services;
using System.Runtime.CompilerServices;

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

        [HttpPut("CreateNewPlayer")]
        public async Task CreateNewPlayer(string name, string password, string email) =>
            await _playersService.CreateNewPlayerAsync(name, password, email);

        [HttpGet("GetTries")]
        public async Task<int> GetTries(string playerId) =>
            await _gameService.GetTriesAsync(playerId);

        [HttpGet("GetGuesses")]
        public async Task<List<Item>> GetGuesses(string playerId) =>
            await _gameService.GetGuessesAsync(playerId);

        [HttpGet("GetActiveGamemode")]
        public async Task<Gamemode?> GetActiveGamemode(string playerId) =>
            await _gameService.GetActiveGamemodeAsync(playerId);

        [HttpGet("GetTargetItemName")]
        public async Task<string> GetTargetItemAsync(string playerId)=>
            await _gameService.GetTargetItemNameAsync(playerId);

        [HttpGet("GetGuess")]
        public async Task<Item> GetGuess(string itemId) =>
            await _itemsService.GetAsync(itemId);

        [HttpGet("GetHints")]
        public async Task<Hints> GetHints(string playerId, string itemId) =>
            await _gameService.GetHintsAsync(playerId, itemId);

        [HttpGet("GetAllHints")]
        public async Task<List<Hints>> GetHints(string playerId) =>
            await _gameService.GetHintsAsync(playerId);
    }
}
