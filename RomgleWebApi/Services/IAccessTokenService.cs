﻿using RomgleWebApi.Data.Models.Auth;
using System.Security.Claims;

namespace RomgleWebApi.Services
{
    public interface IAccessTokenService
    {
        Task<string> GenerateAccessTokenAsync(string playerId);

        Task<RefreshToken> GenerateRefreshTokenAsync();

        Task<ClaimsPrincipal?> ValidateAccessTokenAsync(string accessToken, bool ignoreExpiration = false);
    }
}
