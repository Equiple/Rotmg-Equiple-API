using Microsoft.Extensions.DependencyInjection;
using Moq;
using RotmgleWebApi;
using RotmgleWebApi.Authentication;
using RotmgleWebApi.Items;
using RotmgleWebApiTests.Utils;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
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
            (HttpResponseMessage response, AuthenticationResponse? model) = await AuthenticateGuest(client);

            //Assert
            AssertAuthenticatedResponse(response, model);
        }

        [Test]
        public async Task ValidProviderAuthenticateRequest_ReturnsAuthenticatedResponse()
        {
            //Arrange
            (IAuthenticationValidator validator, AuthenticationPermit permit) =
                ArrangeAuthenticationValidator(isValid: true);
            ArrangeServices(services =>
            {
                services.AddAuthenticationService()
                    .AddValidator(validator);
            });
            HttpClient client = GetClient();

            //Act
            (HttpResponseMessage response, AuthenticationResponse? model) = await Authenticate(client, permit);

            //Assert
            AssertAuthenticatedResponse(response, model);
        }

        [Test]
        public async Task InvalidProviderAuthenticateRequest_ReturnsNotAuthenticatedResponse()
        {
            //Arrange
            (IAuthenticationValidator authenValidator, AuthenticationPermit permit) =
                ArrangeAuthenticationValidator(isValid: false);
            ArrangeServices(services =>
            {
                services.AddAuthenticationService()
                    .AddValidator(authenValidator);
            });
            HttpClient client = GetClient();

            //Act
            (HttpResponseMessage response, AuthenticationResponse? model) = await Authenticate(client, permit);

            //Assert
            AssertNotAuthenticatedResponse(response, model);
        }

        [Test]
        public async Task SelfProviderAuthenticateRequest_ReturnsNotAuthenticatedResponse()
        {
            //Arrange
            AuthenticationPermit permit = new()
            {
                Provider = IdentityProvider.Self,
                IdToken = "WhateverToken",
            };
            HttpClient client = GetClient();

            //Act
            (HttpResponseMessage response, AuthenticationResponse? model) = await Authenticate(client, permit);

            //Assert
            AssertNotAuthenticatedResponse(response, model);
        }

        [Test]
        public async Task AuthorizedRequestToSecuredEndpoint_Returns200()
        {
            //Arrange
            (IAuthenticationValidator authenValidator, AuthenticationPermit permit) =
                ArrangeAuthenticationValidator(isValid: true);
            ArrangeServices(services =>
            {
                services.AddAuthenticationService()
                    .AddValidator(authenValidator);
            });
            HttpClient client = GetClient();

            //Act
            (HttpResponseMessage authenResponse, AuthenticationResponse? authenModel) =
                await Authenticate(client, permit);

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
        public async Task LoggedOutRefreshTokenRequest_Returns401()
        {
            //Arrange
            (IAuthenticationValidator authenValidator, AuthenticationPermit permit) =
                ArrangeAuthenticationValidator(isValid: true);
            ArrangeServices(services =>
            {
                services.AddAuthenticationService()
                    .AddValidator(authenValidator);
            });
            HttpClient client = GetClient();

            //Act
            (HttpResponseMessage authenResponse, AuthenticationResponse? authenModel) =
                await Authenticate(client, permit);

            HttpRequestMessage logoutRequest = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("Authentication/Logout", UriKind.Relative),
            };
            logoutRequest.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer", authenModel?.AccessToken);
            HttpResponseMessage logoutResponse = await client.SendAsync(logoutRequest);

            (HttpResponseMessage refreshResponse, AuthenticationResponse? refreshModel) =
                await RefreshToken(client, authenModel?.AccessToken, authenModel?.RefreshToken);

            //Assert
            Assert.Multiple(() =>
            {
                AssertAuthenticatedResponse(authenResponse, authenModel, suffix: "auth");
                Assert.That(
                    logoutResponse.StatusCode,
                    Is.EqualTo(HttpStatusCode.OK),
                    $"{AssertMessages.StatusCode} (logout)");
                AssertNotAuthenticatedResponse(refreshResponse, refreshModel, suffix: "refresh");
            });
        }

        [Test]
        public async Task AddIdentityRequest_PlayerGetsTwoAuthenticatedIdentities()
        {
            //Arrange
            string googleIdentityId = "TestGoogleId";
            (IAuthenticationValidator googleValidator, AuthenticationPermit googlePermit) =
                ArrangeAuthenticationValidator(
                    isValid: true,
                    identityProvider: IdentityProvider.Google,
                    identityId: googleIdentityId);

            string discordIdentityId = "TestDiscordId";
            (IAuthenticationValidator discordValidator, AuthenticationPermit discordPermit) =
                ArrangeAuthenticationValidator(
                    isValid: true,
                    identityProvider: IdentityProvider.Discord,
                    identityId: discordIdentityId);

            ArrangeServices(services =>
            {
                services.AddAuthenticationService()
                    .AddValidator(googleValidator)
                    .AddValidator(discordValidator);
            });
            HttpClient client = GetClient();

            //Act
            (HttpResponseMessage authenResponse, AuthenticationResponse? authenModel) =
                await Authenticate(client, googlePermit);
            (HttpResponseMessage addIdentityResponse, AuthenticationResponse? addIdentityAuthenModel) =
                await AddIdentity(client, authenModel?.AccessToken, discordPermit);

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

        private static async Task<(HttpResponseMessage, AuthenticationResponse?)> AuthenticateGuest(
            HttpClient client)
        {
            HttpResponseMessage response = await client.PostAsync("Authentication/AuthenticateGuest", null);
            AuthenticationResponse? model = await GetAuthenticationResponseModel(response);
            return (response, model);
        }

        private static async Task<(HttpResponseMessage, AuthenticationResponse?)> Authenticate(
            HttpClient client,
            AuthenticationPermit permit)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync("Authentication/Authenticate", permit);
            AuthenticationResponse? model = await GetAuthenticationResponseModel(response);
            return (response, model);
        }

        private static async Task<(HttpResponseMessage, AuthenticationResponse?)> AddIdentity(
            HttpClient client,
            string? accessToken,
            AuthenticationPermit permit)
        {
            HttpRequestMessage addIdentityRequest = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("Authentication/Authenticate", UriKind.Relative),
                Content = JsonContent.Create(permit),
            };
            addIdentityRequest.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer", accessToken);
            HttpResponseMessage response = await client.SendAsync(addIdentityRequest);
            AuthenticationResponse? model = await GetAuthenticationResponseModel(response);
            return (response, model);
        }

        private static async Task<(HttpResponseMessage, AuthenticationResponse?)> RefreshToken(
            HttpClient client,
            string? accessToken,
            string? refreshToken)
        {
            HttpRequestMessage refreshTokenRequest = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("Authentication/RefreshAccessToken", UriKind.Relative),
                Content = new StringContent(
                    $"\"{refreshToken}\"",
                    Encoding.UTF8,
                    "application/json"),
            };
            refreshTokenRequest.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer", accessToken);
            HttpResponseMessage response = await client.SendAsync(refreshTokenRequest);
            AuthenticationResponse? model = await GetAuthenticationResponseModel(response);
            return (response, model);
        }

        private static async Task<AuthenticationResponse?> GetAuthenticationResponseModel(
            HttpResponseMessage response)
        {
            AuthenticationResponse? model;
            try
            {
                model = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
            }
            catch (JsonException)
            {
                model = null;
            }
            return model;
        }

        private static (IAuthenticationValidator, AuthenticationPermit) ArrangeAuthenticationValidator(
            bool isValid,
            IdentityProvider identityProvider = IdentityProvider.Google,
            string identityId = "TestIdentityId")
        {
            AuthenticationPermit permit = new()
            {
                Provider = identityProvider,
                IdToken = "ValidIdToken",
            };
            Identity identity = new()
            {
                Provider = identityProvider,
                Id = identityId,
            };
            const string name = "TestName";
            Result<AuthenticationValidatorResult> result = isValid
                ? new AuthenticationValidatorResult(identity, name)
                : new Exception("Invalid id token");
            Mock<IAuthenticationValidator> validatorMock = new();
            validatorMock
                .SetupGet(validator => validator.IdentityProvider)
                .Returns(identityProvider);
            validatorMock
                .Setup(validator => validator.ValidateAsync(It.Is<AuthenticationPermit>(
                    p => p.IdToken == permit.IdToken)))
                .Returns(Task.FromResult(result));

            return (validatorMock.Object, permit);
        }

        private static void AssertAuthenticatedResponse(
            HttpResponseMessage response,
            AuthenticationResponse? model,
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
            AuthenticationResponse? model,
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
