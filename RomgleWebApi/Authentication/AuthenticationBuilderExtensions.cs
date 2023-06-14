using Microsoft.AspNetCore.Authentication;

namespace RotmgleWebApi.Authentication
{
    public static class AuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddToken<TIdentityProvider, TUserService, TSessionService>(
            this AuthenticationBuilder builder,
            Action<TokenAuthenticationOptions>? configure)
            where TIdentityProvider : struct, Enum
            where TUserService : class, IUserService<TIdentityProvider>
            where TSessionService : class, ISessionService
        {
            builder.AddScheme<TokenAuthenticationOptions, TokenAuthenticationHandler<TIdentityProvider>>(
                TokenAuthenticationDefaults.Scheme,
                configure);
            IServiceCollection services = builder.Services;
            services.Configure(configure);
            services.AddSingleton<IUserService<TIdentityProvider>, TUserService>();
            services.AddSingleton<ISessionService, TSessionService>();
            services.AddSingleton<
                ITokenAuthenticationService<TIdentityProvider>,
                TokenAuthenticationService<TIdentityProvider>>();
            return builder;
        }
    }
}
