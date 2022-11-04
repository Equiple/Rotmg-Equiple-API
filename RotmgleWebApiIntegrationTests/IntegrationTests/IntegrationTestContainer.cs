using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using RomgleWebApi.Services;
using RotmgleWebApiTests.Mocks.Services;

namespace RotmgleWebApiTests.IntegrationTests
{
    internal abstract class IntegrationTestContainer
    {
        private WebApplicationFactory<Program>? _factory = null;

        private readonly InMemoryPlayersService _playersService = new InMemoryPlayersService();

        protected IPlayersServiceMock PlayersServiceMock => _playersService;

        [SetUp]
        public void BaseSetup()
        {
            _playersService.SetInitialPlayers();
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
            if (_factory == null)
            {
                InitializeFactory(SetupServices);
            }

            options ??= new WebApplicationFactoryClientOptions();
            return _factory!.CreateClient(options);
        }

        private void InitializeFactory(params Action<IServiceCollection>[] configurations)
        {
            _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IPlayerService>(_playersService);

                    foreach (Action<IServiceCollection> configuration in configurations)
                    {
                        configuration.Invoke(services);
                    }
                });
            });
        }

        private void DisposeFactory()
        {
            _factory?.Dispose();
            _factory = null;
        }
    }
}
