using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomgleWebApi.Authentication.Models;
using RomgleWebApi.Authentication.Services;
using RomgleWebApi.ModelBinding.Attributes;
using RomgleWebApi.Utils;

namespace RomgleWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost("AuthenticateGuest")]
        public async Task<AuthenticationResponse> AuthenticateGuest()
        {
            AuthenticationResult result = await _authenticationService.AuthenticateGuestAsync();
            return result;
        }

        [Authorize]
        [HttpPost("Authenticate")]
        public async Task<AuthenticationResponse> Authenticate(
            [UserId] string playerId,
            [DeviceId] string deviceId,
            AuthenticationPermit permit)
        {
            AuthenticationResult result = await _authenticationService.AuthenticateAsync(
                playerId,
                deviceId,
                permit);
            return result;
        }

        [Authorize(Policy = PolicyNames.IgnoreExpiration)]
        [HttpPost("RefreshAccessToken")]
        public async Task<AuthenticationResponse> RefreshAccessToken(
            [UserId] string playerId,
            [DeviceId] string deviceId,
            string refreshToken)
        {
            AuthenticationResult result = await _authenticationService.RefreshAccessTokenAsync(
                playerId,
                deviceId,
                refreshToken);
            return result;
        }

        [Authorize]
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout(
            [UserId] string playerId,
            [DeviceId] string deviceId)
        {
            await _authenticationService.LogoutAsync(playerId, deviceId);
            return Ok();
        }
    }
}
