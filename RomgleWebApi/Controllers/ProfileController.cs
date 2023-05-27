using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RotmgleWebApi.Games;
using RotmgleWebApi.ModelBinding;
using RotmgleWebApi.Players;

namespace RotmgleWebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/profile")]
    public class ProfileController : Controller
    {
        private readonly IPlayerService _playerService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            IPlayerService playerService,
            ILogger<ProfileController> logger)
        {
            _playerService = playerService;
            _logger = logger;
        }

        [HttpGet("GetPlayerStats")]
        public async Task<GameStatisticDetailed> GetPlayerStats([UserId] string playerId, Gamemode mode)
        {
            return await _playerService.GetPlayerStatsAsync(playerId, mode);
        }

        [HttpGet("GetPlayerProfile")]
        public async Task<PlayerProfile> GetPlayerProfile([UserId] string playerId)
        {
            return await _playerService.GetPlayerProfileAsync(playerId);
        }
    }
}
