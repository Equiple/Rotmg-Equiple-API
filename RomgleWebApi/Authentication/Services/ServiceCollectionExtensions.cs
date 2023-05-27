﻿namespace RotmgleWebApi.Authentication
{
    public static class ServiceCollectionExtensions
    {
        public static IAuthenticationServiceBuilder AddAuthenticationService(this IServiceCollection services)
        {
            services.AddSingleton<IAuthenticationService, AuthenticationService>();
            AuthenticationServiceBuilder builder = new(services);
            return builder;
        }

        private class AuthenticationServiceBuilder : IAuthenticationServiceBuilder
        {
            private readonly IServiceCollection _services;

            public AuthenticationServiceBuilder(IServiceCollection services)
            {
                _services = services;
            }

            public IAuthenticationServiceBuilder AddValidator<T>() where T : class, IAuthenticationValidator
            {
                _services.AddSingleton<IAuthenticationValidator, T>();
                return this;
            }
        }
    }
}
