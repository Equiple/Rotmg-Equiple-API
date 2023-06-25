using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace RotmgleWebApi.Authentication
{
    public class TokenAuthenticationService<TIdentityProvider> : ITokenAuthenticationService<TIdentityProvider>
        where TIdentityProvider : struct, Enum
    {
        private static readonly Guid CookieSessionRefreshesId = Guid.NewGuid();

        private readonly IUserService<TIdentityProvider> _userService;
        private readonly ISessionService _sessionService;

        private readonly IEnumerable<IAuthenticationValidator<TIdentityProvider>> _validators;

        private readonly IDeviceIdProviderCollection _deviceIdProviderCollection;

        private readonly TokenAuthenticationOptions _options;

        private readonly IMemoryCache _memoryCache;

        private readonly object _cookieSessionLock = new();

        public TokenAuthenticationService(
            IUserService<TIdentityProvider> userService,
            ISessionService sessionService,
            IEnumerable<IAuthenticationValidator<TIdentityProvider>> validators,
            IDeviceIdProviderCollection deviceIdProviderCollection,
            IOptions<TokenAuthenticationOptions> options,
            IMemoryCache memoryCache)
        {
            _userService = userService;
            _sessionService = sessionService;
            _validators = validators;
            _deviceIdProviderCollection = deviceIdProviderCollection;
            _options = options.Value;
            _memoryCache = memoryCache;
        }

        public async Task<TokenAuthenticationResult> AuthenticateGuestAsync(
            HttpContext context,
            TokenAuthenticationResultType resultType)
        {
            User<TIdentityProvider> user = await _userService.CreateUserAsync(null, null);
            (_, TokenAuthenticationResult result) = await CreateSession(resultType, user.Id, context);
            return result;
        }

        public async Task<Result<TokenAuthenticationResult>> AuthenticateAsync(
            HttpContext context,
            AuthenticationPermit<TIdentityProvider> permit)
        {
            IAuthenticationValidator<TIdentityProvider>? validator = _validators
                .FirstOrDefault(validator => validator.IdentityProvider.Equals(permit.Provider));
            if (validator == null)
            {
                return new Exception("Identity provider not supported");
            }

            Result<AuthenticationValidatorResult<TIdentityProvider>> validationResult =
                await validator.ValidateAsync(permit);
            AuthenticationValidatorResult<TIdentityProvider> validationRes;
            switch (validationResult)
            {
                case Result<AuthenticationValidatorResult<TIdentityProvider>>.Ok ok:
                    validationRes = ok.Value;
                    break;
                case Result<AuthenticationValidatorResult<TIdentityProvider>>.Error error:
                    return error.Exception;
                default:
                    throw new NotSupportedException();
            }

            string? loggedUserId = GetUserId(context);
            User<TIdentityProvider>? loggedUser = loggedUserId != null
                ? await _userService.GetUserAsync(loggedUserId)
                : null;
            User<TIdentityProvider>? user = await _userService.GetUserAsync(validationRes.Identity);

            TokenAuthenticationResult result;

            if (user != null)
            {
                if (loggedUser != null && !loggedUser.IsGuest)
                {
                    if (loggedUser.Id == user.Id)
                    {
                        return new Exception("Already logged in");
                    }
                    return new Exception("Logout first");
                }
                (_, result) = await CreateSession(permit.ResultType, user.Id, context);
                return result;
            }

            if (loggedUser != null)
            {
                string name = loggedUser.Name;
                if (loggedUser.IsGuest && validationRes.Name != null)
                {
                    name = validationRes.Name;
                }
                loggedUser = loggedUser with
                {
                    Name = name,
                    Identities = loggedUser.Identities.Append(validationRes.Identity).ToList(),
                };
                await _userService.UpdateUserAsync(loggedUser);
                (_, result) = await CreateSession(permit.ResultType, loggedUser.Id, context);
                return result;
            }

            user = await _userService.CreateUserAsync(
                validationRes.Name,
                validationRes.Identity);
            (_, result) = await CreateSession(permit.ResultType, user.Id, context);
            return result;
        }

        public async Task<Result<IEnumerable<Claim>, AccessTokenValidationException>> ValidateAccessTokenAsync(
            HttpContext context)
        {
            bool fromCookie = false;
            if (!TryGetAccessTokenFromHeader(context, out string accessToken)
                && !(fromCookie = TryGetAccessTokenFromCookie(context, out accessToken)))
            {
                return new AccessTokenValidationException(null, tokenNotFound: true);
            }

            Session? session = fromCookie
                ? await GetCookieSession(accessToken)
                : await _sessionService.GetSessionAsync(accessToken);

            if (session == null)
            {
                return new AccessTokenValidationException("Session expired or doesn't exist");
            }

            string deviceId = _deviceIdProviderCollection.GetFirstDefinedOrDefaultDeviceId(context);
            if (session.DeviceId != deviceId)
            {
                return new AccessTokenValidationException("Invalid device id");
            }

            if (session.IsExpired)
            {
                return new AccessTokenValidationException("Session expired");
            }

            bool isRefreshTokenRequest = _options.RefreshTokenRequestPath != null
                && context.Request.Path.Value is string path
                && path.StartsWith($"/{_options.RefreshTokenRequestPath}");
            if (!isRefreshTokenRequest && session.IsAccessExpired)
            {
                if (fromCookie)
                {
                    session = await RefreshCookieSession(accessToken, session.UserId, context);
                }
                else
                {
                    return new AccessTokenValidationException("Access expired");
                }
            }

            List<Claim> claims = new()
            {
                new Claim(CustomClaimNames.UserId, session.UserId),
                new Claim(CustomClaimNames.DeviceId, session.DeviceId),
            };
            foreach (KeyValuePair<string, string> item in session.Payload)
            {
                claims.Add(new Claim(item.Key, item.Value));
            }
            return claims;
        }

        public async Task<Result<TokenAuthenticationResult>> RefreshAccessTokenAsync(
            HttpContext context,
            string refreshToken)
        {
            string? userId = GetUserId(context);

            if (userId == null)
            {
                return new Exception("Not authenticated");
            }

            string deviceId = _deviceIdProviderCollection.GetFirstDefinedOrDefaultDeviceId(context);
            IEnumerable<Session> sessions = await _sessionService.GetUserSessionsAsync(userId, deviceId);

            if (!sessions.Any())
            {
                return new Exception("No active session");
            }

            Session? session = sessions.FirstOrDefault(session => session.RefreshToken == refreshToken);

            if (session == null)
            {
                await _sessionService.RemoveUserSessionsAsync(userId, deviceId);
                return new Exception("Invalid refresh token");
            }

            if (session.IsExpired)
            {
                await _sessionService.RemoveUserSessionsAsync(userId, deviceId);
                return new Exception("Session expired");
            }

            (_, TokenAuthenticationResult result) = await CreateSession(
                TokenAuthenticationResultType.Tokens,
                userId,
                context);
            return result;
        }

        public async Task LogoutAsync(HttpContext context)
        {
            string? userId = GetUserId(context);
            if (userId == null)
            {
                return;
            }
            string deviceId = _deviceIdProviderCollection.GetFirstDefinedOrDefaultDeviceId(context);
            await _sessionService.RemoveUserSessionsAsync(userId, deviceId);
            DeleteCookie(context);
        }

        private async Task<Session?> GetCookieSession(string accessToken)
        {
            Task<NewSession>? refresh;
            lock (_cookieSessionLock)
            {
                _memoryCache.TryGetValue(
                    CookieSessionRefreshId(accessToken),
                    out refresh);
            }

            Session? session;
            if (refresh != null)
            {
                (session, _) = await refresh;
            }
            else
            {
                session = await _sessionService.GetSessionAsync(accessToken);
            }
            return session;
        }

        private async Task<Session> RefreshCookieSession(
            string accessToken,
            string userId,
            HttpContext context)
        {
            Task<NewSession> refresh;
            lock (_cookieSessionLock)
            {
                string refreshId = CookieSessionRefreshId(accessToken);
                if (!_memoryCache.TryGetValue(refreshId, out refresh))
                {
                    refresh = CreateSession(
                        TokenAuthenticationResultType.Cookie,
                        userId,
                        context);
                    _memoryCache.Set(
                        refreshId,
                        refresh,
                        DateTimeOffset.UtcNow.AddMinutes(5));
                }
            }

            (Session session, _) = await refresh;
            return session;
        }

        private static string CookieSessionRefreshId(string accessToken)
        {
            return $"{CookieSessionRefreshesId}_{accessToken}";
        }

        private async Task<NewSession> CreateSession(
            TokenAuthenticationResultType type,
            string userId,
            HttpContext context)
        {
            string deviceId = _deviceIdProviderCollection.GetFirstDefinedOrDefaultDeviceId(context);
            await _sessionService.RemoveUserSessionsAsync(userId, deviceId);
            string accessToken = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(_options.AccessTokenByteLength));
            string refreshToken = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(_options.RefreshTokenByteLength));
            IReadOnlyDictionary<string, string> payload = await _userService.CreateSessionPayloadAsync(userId);
            Session session = new(
                AccessToken: accessToken,
                RefreshToken: refreshToken,
                AccessExpiresAt: DateTime.UtcNow.AddMinutes(_options.AccessTokenLifetimeMinutes),
                ExpiresAt: DateTime.UtcNow.AddDays(_options.RefreshTokenLifetimeDays),
                UserId: userId,
                DeviceId: deviceId,
                Payload: payload);
            await _sessionService.AddSessionAsync(session);

            TokenAuthenticationResult result;
            switch (type)
            {
                case TokenAuthenticationResultType.Tokens:
                    result = new TokenAuthenticationResult(
                        Type: TokenAuthenticationResultType.Tokens,
                        AccessToken: accessToken,
                        RefreshToken: refreshToken);
                    break;

                case TokenAuthenticationResultType.Cookie:
                    SetCookie(context, accessToken);
                    result = new TokenAuthenticationResult(
                        Type: TokenAuthenticationResultType.Cookie);
                    break;

                default:
                    throw new NotSupportedException();
            }

            return new NewSession(session, result);
        }

        private static string? GetUserId(HttpContext context)
        {
            Claim? userIdClaim = context.User.Claims
                .FirstOrDefault(x => x.Type == CustomClaimNames.UserId);
            return userIdClaim?.Value;
        }

        private static bool TryGetAccessTokenFromHeader(HttpContext context, out string accessToken)
        {
            accessToken = "";
            string authHeader = context.Request.Headers.Authorization;
            if (string.IsNullOrWhiteSpace(authHeader))
            {
                return false;
            }
            Match tokenMatch = Regex.Match(authHeader, @"(?<=^[Bb]earer\s+)\S+$");
            if (!tokenMatch.Success)
            {
                return false;
            }
            accessToken = tokenMatch.Value;
            return true;
        }

        private static bool TryGetAccessTokenFromCookie(HttpContext context, out string accessToken)
        {
            accessToken = "";
            if (!context.Request.Cookies.TryGetValue(CustomCookieNames.AccessToken, out string? tokenCookie)
                || tokenCookie == null)
            {
                return false;
            }
            accessToken = tokenCookie;
            return true;
        }

        private void SetCookie(HttpContext context, string accessToken)
        {
            context.Response.Cookies.Append(
                CustomCookieNames.AccessToken,
                accessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    IsEssential = true,
                    Expires = DateTimeOffset.UtcNow.AddDays(_options.RefreshTokenLifetimeDays),
                });
        }

        private static void DeleteCookie(HttpContext context)
        {
            context.Response.Cookies.Delete(CustomCookieNames.AccessToken);
        }

        private readonly record struct NewSession(
            Session Session,
            TokenAuthenticationResult Result);
    }
}
