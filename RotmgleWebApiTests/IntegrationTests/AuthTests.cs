using Microsoft.Extensions.DependencyInjection;
using Moq;
using RotmgleWebApi;
using RotmgleWebApi.Authentication;
using RotmgleWebApi.AuthenticationImplementation;
using RotmgleWebApi.Items;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace RotmgleWebApiTests.IntegrationTests
{
    internal class AuthTests : IntegrationTestContainer
    {
        private const string AuthenticateResponse = "Authenticate response";
        private const string IsAuthenticated = "Authenticated";
        private const string AccessTokenIncluded = "Access token included";
        private const string RefreshTokenIncluded = "Refresh token included";
        private const string AccessTokenOmitted = "Access token omitted";
        private const string RefreshTokenOmitted = "Refresh token omitted";

        protected override void Setup()
        {
        }

        [Test]
        public async Task UnauthorizedRequestToSecuredEndpoint_Returns401()
        {
            //Arrange
            HttpClient client = GetClient();

            //Act
            HttpResponseMessage response = await client.GetAsync("GetTries");

            //Assert
            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.Unauthorized),
                AssertMessages.StatusCode);
        }

        [Test]
        public async Task UnauthorizedRequestToUnsecuredEndpoint_Returns200()
        {
            //Arrange
            Mock<IItemService> itemsServiceMock = new();
            itemsServiceMock
                .Setup(service => service.FindAllAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(Task.FromResult((IEnumerable<Item>)new List<Item>()));
            ArrangeServices(services =>
            {
                services.AddSingleton(itemsServiceMock.Object);
            });
            HttpClient client = GetClient();

            //Act
            HttpResponseMessage response = await client.GetAsync("FindAll?searchInput=test&reskinsExcluded=false");

            //Assert
            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.OK),
                AssertMessages.StatusCode);
        }

        [Test]
        public async Task AuthenticateGuestRequest_ReturnsAuthenticatedResponse()
        {
            //Arrange
            HttpClient client = GetClient();

            //Act
            (HttpResponseMessage response, TokenAuthenticationResponse? model) = await AuthenticateGuest(client);

            //Assert
            AssertAuthenticatedResponse(response, model);
        }

        [Test]
        public async Task ValidProviderAuthenticateRequest_ReturnsAuthenticatedResponse()
        {
            //Arrange
            (IAuthenticationValidator<IdentityProvider> validator, TokenAuthenticationRequest request) =
                ArrangeAuthenticationValidator(isValid: true);
            ArrangeServices(services =>
            {
                services.AddAuthenticationValidators<IdentityProvider>()
                    .AddValidator(validator);
            });
            HttpClient client = GetClient();

            //Act
            (HttpResponseMessage response, TokenAuthenticationResponse? model) = await Authenticate(client, request);

            //Assert
            AssertAuthenticatedResponse(response, model);
        }

        [Test]
        public async Task InvalidProviderAuthenticateRequest_ReturnsNotAuthenticatedResponse()
        {
            //Arrange
            (IAuthenticationValidator<IdentityProvider> validator, TokenAuthenticationRequest request) =
                ArrangeAuthenticationValidator(isValid: false);
            ArrangeServices(services =>
            {
                services.AddAuthenticationValidators<IdentityProvider>()
                    .AddValidator(validator);
            });
            HttpClient client = GetClient();

            //Act
            (HttpResponseMessage response, TokenAuthenticationResponse? model) = await Authenticate(client, request);

            //Assert
            AssertNotAuthenticatedResponse(response, model);
        }

        //[Test]
        //public async Task SelfProviderAuthenticateRequest_ReturnsNotAuthenticatedResponse()
        //{
        //    //Arrange
        //    AuthenticationPermit permit = new()
        //    {
        //        Provider = IdentityProvider.Self,
        //        IdToken = "WhateverToken",
        //    };
        //    HttpClient client = GetClient();

        //    //Act
        //    (HttpResponseMessage response, TokenAuthenticationResponse? model) = await Authenticate(client, permit);

        //    //Assert
        //    AssertNotAuthenticatedResponse(response, model);
        //}

        [Test]
        public async Task AuthorizedRequestToSecuredEndpoint_Returns200()
        {
            //Arrange
            (IAuthenticationValidator<IdentityProvider> validator, TokenAuthenticationRequest request) =
                ArrangeAuthenticationValidator(isValid: true);
            ArrangeServices(services =>
            {
                services.AddAuthenticationValidators<IdentityProvider>()
                    .AddValidator(validator);
            });
            HttpClient client = GetClient();

            //Act
            (HttpResponseMessage authenResponse, TokenAuthenticationResponse? authenModel) =
                await Authenticate(client, request);

            HttpRequestMessage securedEndpointRequest = new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("GetTries", UriKind.Relative),
            };
            securedEndpointRequest.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer", authenModel?.AccessToken);

            HttpResponseMessage securedEndpointResponse = await client.SendAsync(securedEndpointRequest);

            //Assert
            Assert.Multiple(() =>
            {
                AssertAuthenticatedResponse(authenResponse, authenModel, suffix: "auth");
                Assert.That(
                    securedEndpointResponse.StatusCode,
                    Is.EqualTo(HttpStatusCode.OK),
                    AssertMessages.StatusCode);
            });
        }

        [Test]
        public async Task LoggedOutRequestToSecuredEndpoint_Returns401()
        {
            //Arrange
            (IAuthenticationValidator<IdentityProvider> validator, TokenAuthenticationRequest request) =
                ArrangeAuthenticationValidator(isValid: true);
            ArrangeServices(services =>
            {
                services.AddAuthenticationValidators<IdentityProvider>()
                    .AddValidator(validator);
            });
            HttpClient client = GetClient();

            //Act
            (HttpResponseMessage authenResponse, TokenAuthenticationResponse? authenModel) =
                await Authenticate(client, request);
            AuthenticationHeaderValue authorizationHeader = new("Bearer", authenModel?.AccessToken);

            HttpRequestMessage logoutRequest = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("Authentication/Logout", UriKind.Relative),
            };
            logoutRequest.Headers.Authorization = authorizationHeader;
            HttpResponseMessage logoutResponse = await client.SendAsync(logoutRequest);

            HttpRequestMessage securedEndpointRequest = new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("GetTries", UriKind.Relative)
            };
            securedEndpointRequest.Headers.Authorization = authorizationHeader;
            HttpResponseMessage securedEndpointResponse = await client.SendAsync(securedEndpointRequest);

            //Assert
            Assert.Multiple(() =>
            {
                AssertAuthenticatedResponse(authenResponse, authenModel, suffix: "auth");
                Assert.That(
                    logoutResponse.StatusCode,
                    Is.EqualTo(HttpStatusCode.OK),
                    $"{AssertMessages.StatusCode} (logout)");
                Assert.That(
                    securedEndpointResponse.StatusCode,
                    Is.EqualTo(HttpStatusCode.Unauthorized),
                    $"{AssertMessages.StatusCode} (secured endpoint)");
            });
        }

        [Test]
        public async Task AddIdentityRequest_PlayerGetsTwoAuthenticatedIdentities()
        {
            //Arrange
            string googleIdentityId = "TestGoogleId";
            (IAuthenticationValidator<IdentityProvider> googleValidator, TokenAuthenticationRequest googleRequest) =
                ArrangeAuthenticationValidator(
                    isValid: true,
                    identityProvider: IdentityProvider.Google,
                    identityId: googleIdentityId);

            string discordIdentityId = "TestDiscordId";
            (IAuthenticationValidator<IdentityProvider> discordValidator, TokenAuthenticationRequest discordRequest) =
                ArrangeAuthenticationValidator(
                    isValid: true,
                    identityProvider: IdentityProvider.Discord,
                    identityId: discordIdentityId);

            ArrangeServices(services =>
            {
                services.AddAuthenticationValidators<IdentityProvider>()
                    .AddValidator(googleValidator)
                    .AddValidator(discordValidator);
            });
            HttpClient client = GetClient();

            //Act
            (HttpResponseMessage authenResponse, TokenAuthenticationResponse? authenModel) =
                await Authenticate(client, googleRequest);
            (HttpResponseMessage addIdentityResponse, TokenAuthenticationResponse? addIdentityAuthenModel) =
                await AddIdentity(client, authenModel?.AccessToken, discordRequest);

            //Assert
            Assert.Multiple(() =>
            {
                AssertAuthenticatedResponse(
                    authenResponse,
                    authenModel,
                    suffix: "initial authentication");
                AssertAuthenticatedResponse(
                    addIdentityResponse,
                    addIdentityAuthenModel,
                    suffix: "add identity authentication");
                CollectionAssert.AreEquivalent(
                    new[]
                    {
                        (IdentityProvider.Google, googleIdentityId),
                        (IdentityProvider.Discord, discordIdentityId),
                    },
                    PlayerServiceMock.Players.SingleOrDefault()?.Identities
                        .Select(identity => (identity.Provider, identity.Id)),
                    "Single player has 2 authenticated identities");
            });
        }

        #region helpers

        private static async Task<(HttpResponseMessage, TokenAuthenticationResponse?)> AuthenticateGuest(
            HttpClient client)
        {
            HttpResponseMessage response = await client.PostAsync(
                "Authentication/AuthenticateGuest?resultType=Tokens", null);
            TokenAuthenticationResponse? model = await GetAuthenticationResponseModel(response);
            return (response, model);
        }

        private static async Task<(HttpResponseMessage, TokenAuthenticationResponse?)> Authenticate(
            HttpClient client,
            TokenAuthenticationRequest request)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync(
                "Authentication/Authenticate",
                request,
                options: JsonUtils.DefaultOptions);
            TokenAuthenticationResponse? model = await GetAuthenticationResponseModel(response);
            return (response, model);
        }

        private static async Task<(HttpResponseMessage, TokenAuthenticationResponse?)> AddIdentity(
            HttpClient client,
            string? accessToken,
            TokenAuthenticationRequest request)
        {
            HttpRequestMessage addIdentityRequest = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("Authentication/Authenticate", UriKind.Relative),
                Content = JsonContent.Create(request, options: JsonUtils.DefaultOptions),
            };
            addIdentityRequest.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer", accessToken);
            HttpResponseMessage response = await client.SendAsync(addIdentityRequest);
            TokenAuthenticationResponse? model = await GetAuthenticationResponseModel(response);
            return (response, model);
        }

        private static async Task<(HttpResponseMessage, TokenAuthenticationResponse?)> RefreshToken(
            HttpClient client,
            string? accessToken,
            string? refreshToken)
        {
            HttpRequestMessage refreshTokenRequest = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"Authentication/RefreshAccessToken?refreshToken={refreshToken}", UriKind.Relative),
            };
            refreshTokenRequest.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer", accessToken);
            HttpResponseMessage response = await client.SendAsync(refreshTokenRequest);
            TokenAuthenticationResponse? model = await GetAuthenticationResponseModel(response);
            return (response, model);
        }

        private static async Task<TokenAuthenticationResponse?> GetAuthenticationResponseModel(
            HttpResponseMessage response)
        {
            TokenAuthenticationResponse? model;
            try
            {
                string test = await response.Content.ReadAsStringAsync();
                model = await response.Content.ReadFromJsonAsync<TokenAuthenticationResponse>(
                    options: JsonUtils.DefaultOptions);
            }
            catch (JsonException)
            {
                model = null;
            }
            return model;
        }

        private static (IAuthenticationValidator<IdentityProvider>, TokenAuthenticationRequest) ArrangeAuthenticationValidator(
            bool isValid,
            IdentityProvider identityProvider = IdentityProvider.Google,
            string identityId = "TestIdentityId")
        {
            TokenAuthenticationRequest request = new()
            {
                ResultType = TokenAuthenticationResultType.Tokens,
                Provider = identityProvider,
                IdToken = "ValidIdToken",
            };
            AuthenticationPermit<IdentityProvider> permit = request.ToPermit();
            Identity<IdentityProvider> identity = new(
                Provider: identityProvider,
                Id: identityId);
            const string name = "TestName";
            Result<AuthenticationValidatorResult<IdentityProvider>> result = isValid
                ? new AuthenticationValidatorResult<IdentityProvider>(identity, name)
                : new Exception("Invalid id token");
            Mock<IAuthenticationValidator<IdentityProvider>> validatorMock = new();
            validatorMock
                .SetupGet(validator => validator.IdentityProvider)
                .Returns(identityProvider);
            validatorMock
                .Setup(validator => validator.ValidateAsync(It.Is<AuthenticationPermit<IdentityProvider>>(
                    p => p.IdToken == permit.IdToken)))
                .Returns(Task.FromResult(result));

            return (validatorMock.Object, request);
        }

        private static void AssertAuthenticatedResponse(
            HttpResponseMessage response,
            TokenAuthenticationResponse? model,
            string? suffix = null)
        {
            suffix = !string.IsNullOrWhiteSpace(suffix) ? $" ({suffix})" : "";
            Assert.Multiple(() =>
            {
                Assert.That(
                    response.StatusCode,
                    Is.EqualTo(HttpStatusCode.OK),
                    $"{AssertMessages.StatusCode}{suffix}");
                Assert.That(
                    model,
                    Is.Not.Null,
                    $"{AuthenticateResponse}{suffix}");
                Assert.That(
                    model?.IsAuthenticated,
                    Is.True,
                    $"{IsAuthenticated}{suffix}");
                Assert.That(
                    !string.IsNullOrWhiteSpace(model?.AccessToken),
                    Is.True,
                    $"{AccessTokenIncluded}{suffix}");
                Assert.That(
                    !string.IsNullOrWhiteSpace(model?.RefreshToken),
                    Is.True,
                    $"{RefreshTokenIncluded}{suffix}");
            });
        }

        private static void AssertNotAuthenticatedResponse(
            HttpResponseMessage response,
            TokenAuthenticationResponse? model,
            string? suffix = null)
        {
            suffix = !string.IsNullOrWhiteSpace(suffix) ? $" ({suffix})" : "";
            Assert.Multiple(() =>
            {
                Assert.That(
                    response.StatusCode,
                    Is.EqualTo(HttpStatusCode.OK),
                    $"{AssertMessages.StatusCode}{suffix}");
                Assert.That(
                    model,
                    Is.Not.Null,
                    $"{AuthenticateResponse}{suffix}");
                Assert.That(
                    model?.IsAuthenticated,
                    Is.False,
                    $"{IsAuthenticated}{suffix}");
                Assert.That(
                    string.IsNullOrWhiteSpace(model?.AccessToken),
                    Is.True,
                    $"{AccessTokenOmitted}{suffix}");
                Assert.That(
                    string.IsNullOrWhiteSpace(model?.RefreshToken),
                    Is.True,
                    $"{RefreshTokenOmitted}{suffix}");
            });
        }

        #endregion
    }
}
