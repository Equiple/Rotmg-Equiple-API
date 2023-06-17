using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RotmgleWebApi.Authentication;
using RotmgleWebApi.AuthenticationImplementation;

namespace RotmgleWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly ITokenAuthenticationService<IdentityProvider> _authenticationService;

        public AuthenticationController(ITokenAuthenticationService<IdentityProvider> authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [AllowAnonymous]
        [HttpPost("AuthenticateGuest")]
        public async Task<TokenAuthenticationResponse> AuthenticateGuest(
            [FromQuery] TokenAuthenticationResultType resultType)
        {
            TokenAuthenticationResult result = await _authenticationService.AuthenticateGuestAsync(
                HttpContext,
                resultType);
            return result.ToResponse();
        }

        [Authorize]
        [AllowAnonymous]
        [HttpPost("Authenticate")]
        public async Task<TokenAuthenticationResponse> Authenticate(
            [FromBody] TokenAuthenticationRequest request)
        {
            Result<TokenAuthenticationResult> result = await _authenticationService.AuthenticateAsync(
                HttpContext,
                request.ToPermit());
            return result.ToResponse();
        }

        [Authorize]
        [HttpPost("RefreshAccessToken")]
        public async Task<TokenAuthenticationResponse> RefreshAccessToken([FromQuery] string refreshToken)
        {
            Result<TokenAuthenticationResult> result = await _authenticationService.RefreshAccessTokenAsync(
                HttpContext,
                refreshToken);
            return result.ToResponse();
        }

        [Authorize]
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await _authenticationService.LogoutAsync(HttpContext);
            return Ok();
        }
    }
}
