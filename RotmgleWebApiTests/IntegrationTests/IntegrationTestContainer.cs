using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using RotmgleWebApi;
using RotmgleWebApi.Authentication;
using RotmgleWebApi.Jobs;
using RotmgleWebApi.Players;
using RotmgleWebApiTests.Mocks;

namespace RotmgleWebApiTests.IntegrationTests
{
    internal abstract class IntegrationTestContainer
    {
        private WebApplicationFactory<Program>? _factory = null;

        private readonly InMemorySessionService _sessionService = new();
        private readonly InMemoryPlayerService _playerService = new();

        protected IPlayerServiceMock PlayerServiceMock => _playerService;

        [SetUp]
        public void BaseSetup()
        {
            StaticRegistrationHelper.SetTesting();
            _sessionService.Clear();
            _playerService.SetInitialPlayers();
            Setup();
        }

        [TearDown]
        public void TearDown()
        {
            DisposeFactory();
        }

        protected void ArrangeServices(Action<IServiceCollection> arrangement)
        {
            if (_factory != null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ArrangeServices)} must be called at most once " +
                    $"and before {nameof(GetClient)}");
            }

            DisposeFactory();
            InitializeFactory(SetupServices, arrangement);
        }

        protected abstract void Setup();

        protected virtual void SetupServices(IServiceCollection services)
        { 
        }

        protected HttpClient GetClient(WebApplicationFactoryClientOptions? options = null)
        {
            WebApplicationFactory<Program> factory =
                _factory ?? InitializeFactory(SetupServices);
            options ??= new WebApplicationFactoryClientOptions();
            return factory.CreateClient(options);
        }

        private WebApplicationFactory<Program> InitializeFactory(
            params Action<IServiceCollection>[] configurations)
        {
            _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<ISessionService>(_sessionService);
                    services.AddSingleton<IPlayerService>(_playerService);

                    GlobalConfiguration.Configuration.UseMemoryStorage();
                    RecurringJobInitializer.Initialize();

                    foreach (Action<IServiceCollection> configuration in configurations)
                    {
                        configuration.Invoke(services);
                    }
                });
            });
            return _factory;
        }

        private void DisposeFactory()
        {
            _factory?.Dispose();
            _factory = null;
        }
    }
}
