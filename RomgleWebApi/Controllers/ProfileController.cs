using Microsoft.AspNetCore.Mvc;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Services;

namespace RomgleWebApi.Controllers
{
    [ApiController]
    [Route("/profile")]
    public class ProfileController : Controller
    {
        private readonly PlayersService _playersService;
        private readonly ItemsService _itemsService;
        private readonly ILogger<GameController> _logger;

        public ProfileController(ILogger<GameController> logger, PlayersService playersService, ItemsService itemsService)
        {
            _logger = logger;
            _playersService = playersService;
            _itemsService = itemsService;
        }

        [HttpGet("GetPlayerStats")]
        public async Task<GameStatistic> GetPlayerStats(string playerId, Gamemode mode)
        {
            return await _playersService.GetPlayerStatsAsync(playerId, mode);
        }

        [HttpGet("GetPlayerProfile")]
        public async Task<PlayerProfile> GetPlayerProfile(string playerId)
        {
            return await _playersService.GetPlayerProfileAsync(playerId);
        }
    }
}
