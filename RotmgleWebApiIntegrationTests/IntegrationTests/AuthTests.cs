using Microsoft.Extensions.DependencyInjection;
using Moq;
using RomgleWebApi.Data.Auth;
using RomgleWebApi.Data.Extensions;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Models.Auth;
using RomgleWebApi.IdentityValidators;
using RomgleWebApi.Services;
using RomgleWebApi.Services.ServiceCollectionExtensions;
using RotmgleWebApiTests.Data.Models.Auth;
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
        private const string RefreshTokenIncluded = "Access token included";
        private const string AccessTokenOmitted = "Access token omitted";
        private const string RefreshTokenOmitted = "Access token omitted";

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
            Mock<IItemsService> itemsServiceMock = new Mock<IItemsService>();
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
            Func<Identity, AuthenticationValidatorResult> result)
        {
            AuthenticationPermit permit = new AuthenticationPermit
            {
                Provider = IdentityProvider.Google,
                IdToken = "ValidIdToken"
            };
            Identity identity = permit.CreateIdentity("IdentityId");
            Mock<IAuthenticationValidator> validatorMock = new Mock<IAuthenticationValidator>();
            validatorMock
                .SetupGet(validator => validator.IdentityProvider)
                .Returns(IdentityProvider.Google);
            validatorMock
                .Setup(validator => validator.Validate(It.Is<AuthenticationPermit>(
                    p => p.IdToken == permit.IdToken)))
                .Returns(Task.FromResult(result.Invoke(identity)));

            return (validatorMock.Object, permit);
        }

        private static void AssertAuthenticatedResponse(
            HttpResponseMessage response,
            AuthenticationResponse? model)
        {
            Assert.Multiple(() =>
            {
                Assert.That(
                    response.StatusCode,
                    Is.EqualTo(HttpStatusCode.OK),
                    AssertMessages.StatusCode);
                Assert.That(
                    model,
                    Is.Not.Null,
                    AuthenticateResponse);
                Assert.That(
                    model?.IsAuthenticated,
                    Is.True,
                    IsAuthenticated);
                Assert.That(
                    !string.IsNullOrWhiteSpace(model?.AccessToken),
                    Is.True,
                    AccessTokenIncluded);
                Assert.That(
                    !string.IsNullOrWhiteSpace(model?.RefreshToken),
                    Is.True,
                    RefreshTokenIncluded);
            });
        }

        private static void AssertNotAuthenticatedResponse(
            HttpResponseMessage response,
            AuthenticationResponse? model)
        {
            Assert.Multiple(() =>
            {
                Assert.That(
                    response.StatusCode,
                    Is.EqualTo(HttpStatusCode.OK),
                    AssertMessages.StatusCode);
                Assert.That(
                    model,
                    Is.Not.Null,
                    AuthenticateResponse);
                Assert.That(
                    model?.IsAuthenticated,
                    Is.False,
                    IsAuthenticated);
                Assert.That(
                    string.IsNullOrWhiteSpace(model?.AccessToken),
                    Is.True,
                    AccessTokenOmitted);
                Assert.That(
                    string.IsNullOrWhiteSpace(model?.RefreshToken),
                    Is.True,
                    RefreshTokenOmitted);
            });
        }

        #endregion
    }
}
