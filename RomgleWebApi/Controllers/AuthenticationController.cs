using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RotmgleWebApi.Authentication;
using RotmgleWebApi.ModelBinding;

namespace RotmgleWebApi.Controllers
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

        [AllowAnonymous]
        [HttpPost("AuthenticateGuest")]
        public async Task<AuthenticationResponse> AuthenticateGuest([DeviceId] string deviceId)
        {
            AuthenticationResult result = await _authenticationService.AuthenticateGuestAsync(deviceId);
            return result.ToResponse();
        }

        [Authorize]
        [AllowAnonymous]
        [HttpPost("Authenticate")]
        public async Task<AuthenticationResponse> Authenticate(
            [UserId] string? playerId,
            [DeviceId] string deviceId,
            [FromBody] AuthenticationPermit permit)
        {
            Result<AuthenticationResult> result = await _authenticationService.AuthenticateAsync(
                playerId,
                deviceId,
                permit);
            return result.ToResponse();
        }

        [Authorize]
        [HttpPost("RefreshAccessToken")]
        public async Task<AuthenticationResponse> RefreshAccessToken(
            [UserId] string playerId,
            [DeviceId] string deviceId,
            [FromBody] string refreshToken)
        {
            Result<AuthenticationResult> result = await _authenticationService.RefreshAccessTokenAsync(
                playerId,
                deviceId,
                refreshToken);
            return result.ToResponse();
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
