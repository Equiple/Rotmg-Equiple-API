using Microsoft.AspNetCore.Mvc;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Services;

namespace RomgleWebApi.Controllers
{
    [ApiController]
    [Route("/profile")]
    public class ProfileController : Controller
    {
        private readonly IPlayersService _playersService;
        private readonly IItemsService _itemsService;
        private readonly ILogger<GameController> _logger;

        public ProfileController(
            ILogger<GameController> logger,
            IPlayersService playersService,
            IItemsService itemsService)
        {
            _logger = logger;
            _playersService = playersService;
            _itemsService = itemsService;
        }

        [HttpGet("GetPlayerStats")]
        public async Task<DetailedGameStatistic> GetPlayerStats(string playerId, Gamemode mode)
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
