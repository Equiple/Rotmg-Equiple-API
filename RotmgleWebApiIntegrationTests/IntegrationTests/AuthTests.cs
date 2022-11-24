using Microsoft.Extensions.DependencyInjection;
using Moq;
using RomgleWebApi.Authentication.AuthenticationValidators;
using RomgleWebApi.Data.Auth;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Models.Auth;
using RomgleWebApi.Extensions;
using RomgleWebApi.Services;
using RomgleWebApi.Services.ServiceCollectionExtensions;
using RotmgleWebApiTests.Utils;
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
        private const string DeviceIdIncluded = "Device id included";
        private const string AccessTokenOmitted = "Access token omitted";
        private const string RefreshTokenOmitted = "Refresh token omitted";
        private const string DeviceIdOmitted = "Device id omitted";

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
            Mock<IItemService> itemsServiceMock = new Mock<IItemService>();
            itemsServiceMock
                .Setup(service => service.FindAllAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(Task.FromResult((IReadOnlyList<Item>)new List<Item>()));
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
                ArrangeAuthenticationValidator(identity => AuthenticationValidatorResult.Valid(identity));
            ArrangeServices(services =>
            {
                services.AddAuthenticationService(validator);
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
                ArrangeAuthenticationValidator(_ => AuthenticationValidatorResult.Invalid);
            ArrangeServices(services =>
            {
                services.AddAuthenticationService(authenValidator);
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
            AuthenticationPermit permit = new AuthenticationPermit
            {
                Provider = IdentityProvider.Self,
                IdToken = "WhateverToken"
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
                ArrangeAuthenticationValidator(identity => AuthenticationValidatorResult.Valid(identity));
            ArrangeServices(services =>
            {
                services.AddAuthenticationService(authenValidator);
            });
            HttpClient client = GetClient();

            //Act
            (HttpResponseMessage authenResponse, AuthenticationResponse? authenModel) =
                await Authenticate(client, permit);

            HttpRequestMessage securedEndpointRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("GetTries", UriKind.Relative)
            };
            securedEndpointRequest.Headers.Authorization = AuthenticationHeaderValue.Parse(
                $"Bearer {authenModel?.AccessToken}");

            HttpResponseMessage securedEndpointResponse = await client.SendAsync(securedEndpointRequest);

            //Assert
            Assert.Multiple(() =>
            {
                AssertAuthenticatedResponse(authenResponse, authenModel);
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
            (IAuthenticationValidator authenValidator, AuthenticationPermit permit) =
                ArrangeAuthenticationValidator(identity => AuthenticationValidatorResult.Valid(identity));
            ArrangeServices(services =>
            {
                services.AddAuthenticationService(authenValidator);
            });
            HttpClient client = GetClient();

            //Act
            (HttpResponseMessage authenResponse, AuthenticationResponse? authenModel) =
                await Authenticate(client, permit);
            AuthenticationHeaderValue authorizationHeader = AuthenticationHeaderValue.Parse(
                $"Bearer {authenModel?.AccessToken}");

            HttpRequestMessage logoutRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("Authentication/Logout", UriKind.Relative)
            };
            logoutRequest.Headers.Authorization = authorizationHeader;

            HttpRequestMessage securedEndpointRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("GetTries", UriKind.Relative)
            };
            securedEndpointRequest.Headers.Authorization = authorizationHeader;

            HttpResponseMessage logoutResponse = await client.SendAsync(logoutRequest);
            HttpResponseMessage securedEndpointResponse = await client.SendAsync(securedEndpointRequest);

            //Assert
            Assert.Multiple(() =>
            {
                AssertAuthenticatedResponse(authenResponse, authenModel);
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
            (IAuthenticationValidator googleValidator, AuthenticationPermit googlePermit) =
                ArrangeAuthenticationValidator(
                    identity => AuthenticationValidatorResult.Valid(identity),
                    identityProvider: IdentityProvider.Google,
                    identityId: googleIdentityId);

            string discordIdentityId = "TestDiscordId";
            (IAuthenticationValidator discordValidator, AuthenticationPermit discordPermit) =
                ArrangeAuthenticationValidator(
                    identity => AuthenticationValidatorResult.Valid(identity),
                    identityProvider: IdentityProvider.Discord,
                    identityId: discordIdentityId);

            ArrangeServices(services =>
            {
                services.AddAuthenticationService(googleValidator, discordValidator);
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
                        (IdentityProvider.Discord, discordIdentityId)
                    },
                    PlayersServiceMock.Players.SingleOrDefault()?.Identities
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
            HttpRequestMessage addIdentityRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("Authentication/Authenticate", UriKind.Relative),
                Content = JsonContent.Create(permit)
            };
            addIdentityRequest.Headers.Authorization = AuthenticationHeaderValue.Parse(
                $"Bearer {accessToken}");
            HttpResponseMessage response = await client.SendAsync(addIdentityRequest);
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
            Func<Identity, AuthenticationValidatorResult> result,
            IdentityProvider identityProvider = IdentityProvider.Google,
            string identityId = "TestIdentityId")
        {
            AuthenticationPermit permit = new AuthenticationPermit
            {
                Provider = identityProvider,
                IdToken = "ValidIdToken"
            };
            Identity identity = permit.CreateIdentity(identityId, new IdentityDetails
            {
                Name = "TestName"
            });
            Mock<IAuthenticationValidator> validatorMock = new Mock<IAuthenticationValidator>();
            validatorMock
                .SetupGet(validator => validator.IdentityProvider)
                .Returns(identityProvider);
            validatorMock
                .Setup(validator => validator.ValidateAsync(It.Is<AuthenticationPermit>(
                    p => p.IdToken == permit.IdToken)))
                .Returns(Task.FromResult(result.Invoke(identity)));

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
                    $"{AssertMessages.StatusCode} (authentication{suffix})");
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
                Assert.That(
                    !string.IsNullOrWhiteSpace(model?.DeviceId),
                    Is.True,
                    $"{DeviceIdIncluded}{suffix}");
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
                    $"{AssertMessages.StatusCode} (authentication{suffix})");
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
                Assert.That(
                    string.IsNullOrWhiteSpace(model?.DeviceId),
                    Is.True,
                    $"{DeviceIdOmitted}{suffix}");
            });
        }

        #endregion
    }
}
