using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomgleWebApi.Data.Auth;
using RomgleWebApi.Data.Models.Auth;
using RomgleWebApi.Filters.Attributes;
using RomgleWebApi.ModelBinding.Attributes;
using RomgleWebApi.Services;

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
            IAuthenticationResult result = await _authenticationService.AuthenticateGuest(name);
            return result;
        }

        [HttpPost("Authenticate")]
        public async Task<IAuthenticationResponse> Authenticate(AuthenticationPermit permit)
        {
            IAuthenticationResult result = await _authenticationService.Authenticate(permit);
            return result;
        }

        [Authorize]
        [HttpPost("AddIdentity")]
        public async Task<IIsAuthenticatedResponse> AddIdentity([UserId] string playerId, AuthenticationPermit permit)
        {
            IAuthenticationResult result = await _authenticationService.AddIdentity(playerId, permit);
            return result;
        }

        [AuthorizeIgnoringLifetime]
        [HttpPost("RefreshAccessToken")]
        public async Task<IAuthenticationResponse> RefreshAccessToken([UserId] string playerId, [RefreshToken] string refreshToken)
        {
            IAuthenticationResult result = await _authenticationService.RefreshAccessToken(playerId, refreshToken);
            return result;
        }
    }
}
