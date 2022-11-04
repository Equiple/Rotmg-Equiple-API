using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomgleWebApi.Data.Models;
using RomgleWebApi.ModelBinding.Attributes;
using RomgleWebApi.Services;

namespace RomgleWebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/profile")]
    public class ProfileController : Controller
    {
        private readonly IPlayerService _playersService;
        private readonly IItemService _itemsService;
        private readonly ILogger<GameController> _logger;

        public ProfileController(
            ILogger<GameController> logger,
            IPlayerService playersService,
            IItemService itemsService)
        {
            _logger = logger;
            _playersService = playersService;
            _itemsService = itemsService;
        }

        [HttpGet("GetPlayerStats")]
        public async Task<DetailedGameStatistic> GetPlayerStats([UserId] string playerId, Gamemode mode)
        {
            return await _playersService.GetPlayerStatsAsync(playerId, mode);
        }

        [HttpGet("GetPlayerProfile")]
        public async Task<PlayerProfile> GetPlayerProfile([UserId] string playerId)
        {
            return await _playersService.GetPlayerProfileAsync(playerId);
        }
    }
}
