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
            ItemsService itemsService, PlayersService playersService, DailiesService dailiesService)
        {
            _logger = logger;
            _itemsService = itemsService;
            _playersService = playersService;
            _dailiesService = dailiesService;
        }

        [HttpGet("FindAll")]
        public async Task<List<Item>> FindAll(string searchInput) =>
            await _itemsService.FindAllAsync(searchInput);

        [HttpPost("StartNormal")]
        public async Task StartNormal(string itemId, string playerId) =>
            await _gameService.CheckGuessAsync(itemId, playerId, "Normal");

        [HttpPost("StartDaily")]
        public async Task StartDaily(string itemId, string playerId) =>
            await _gameService.CheckGuessAsync(itemId, playerId, "Daily");

        [HttpPost("CheckGuess")]
        public async Task<GuessResult> CheckGuess(string itemId, string playerId) =>
            await _gameService.CheckGuessAsync(itemId, playerId, "");

        [HttpGet("Daily")]
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

        [HttpGet("HasAnActiveGame")]
        public async Task<bool> HasAnActiveGame(string playerId) =>
            await _gameService.HasAnActiveGameAsync(playerId);

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
