using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomgleWebApi.Data.Auth;
using RomgleWebApi.Data.Models.Auth;
using RomgleWebApi.ModelBinding.Attributes;
using RomgleWebApi.Services;
using RomgleWebApi.Utils;

namespace RomgleWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : Controller
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost("AuthenticateGuest")]
        public async Task<IAuthenticationResponse> AuthenticateGuest(string? name)
        {
            AuthenticationResult result = await _authenticationService.AuthenticateGuest(name);
            return result;
        }

        [HttpPost("Authenticate")]
        public async Task<IAuthenticationResponse> Authenticate(AuthenticationPermit permit)
        {
            AuthenticationResult result = await _authenticationService.Authenticate(permit);
            return result;
        }

        [Authorize]
        [HttpPost("AddIdentity")]
        public async Task<IIsAuthenticatedResponse> AddIdentity([UserId] string playerId, AuthenticationPermit permit)
        {
            AuthenticationResult result = await _authenticationService.AddIdentity(playerId, permit);
            return result;
        }

        [Authorize(Policy = PolicyNames.IgnoreExpiration)]
        [HttpPost("RefreshAccessToken")]
        public async Task<IAuthenticationResponse> RefreshAccessToken([UserId] string playerId, string refreshToken)
        {
            AuthenticationResult result = await _authenticationService.RefreshAccessToken(playerId, refreshToken);
            return result;
        }

        [Authorize]
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout([UserId] string playerId)
        {
            await _authenticationService.Logout(playerId);
            return Ok();
        }
    }
}
