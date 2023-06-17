using RotmgleWebApi.Authentication;
using RotmgleWebApi.Players;

namespace RotmgleWebApi.AuthenticationImplementation
{
    public static class AuthenticationModelsUtils
    {
        public static AuthenticationPermit<IdentityProvider> ToPermit(this TokenAuthenticationRequest request)
        {
            AuthenticationPermit<IdentityProvider> permit = new(
                ResultType: request.ResultType,
                Provider: request.Provider,
                IdToken: request.IdToken,
                AuthCode: request.AuthCode);
            return permit;
        }

        public static TokenAuthenticationResponse ToResponse(this TokenAuthenticationResult result)
        {
            TokenAuthenticationResponse response = new()
            {
                IsAuthenticated = true,
                Type = result.Type,
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken,
            };
            return response;
        }

        public static TokenAuthenticationResponse ToResponse(this Result<TokenAuthenticationResult> result)
        {
            if (result is Result<TokenAuthenticationResult>.Ok ok)
            {
                return ok.Value.ToResponse();
            }
            return new TokenAuthenticationResponse
            {
                IsAuthenticated = false,
            };
        }

        public static User<IdentityProvider> ToUser(this Player player)
        {
            User<IdentityProvider> user = new(
                Id: player.Id,
                Name: player.Name,
                Identities: player.Identities
                    .Select(identityModel => identityModel.ToIdentity())
                    .ToList());
            return user;
        }

        public static Identity<IdentityProvider> ToIdentity(this Identity identityModel)
        {
            Identity<IdentityProvider> identity = new(
                Provider: identityModel.Provider,
                Id: identityModel.Id);
            return identity;    
        }

        public static Identity ToIdentityModel(this Identity<IdentityProvider> identity)
        {
            Identity identityModel = new()
            {
                Provider = identity.Provider,
                Id = identity.Id,
            };
            return identityModel;
        }

        public static Authentication.Session ToSession(this Session sessionModel)
        {
            Authentication.Session session = new(
                AccessToken: sessionModel.AccessToken,
                RefreshToken: sessionModel.RefreshToken,
                AccessExpiresAt: sessionModel.AccessExpiresAt,
                ExpiresAt: sessionModel.ExpiresAt,
                UserId: sessionModel.UserId,
                DeviceId: sessionModel.DeviceId,
                Payload: sessionModel.Payload);
            return session;
        }

        public static Session ToSessionModel(this Authentication.Session session)
        {
            Session sessionModel = new()
            {
                AccessToken = session.AccessToken,
                RefreshToken = session.RefreshToken,
                AccessExpiresAt = session.AccessExpiresAt,
                ExpiresAt = session.ExpiresAt,
                UserId = session.UserId,
                DeviceId = session.DeviceId,
                Payload = session.Payload.ToDictionary(x => x.Key, x => x.Value),
            };
            return sessionModel;
        }
    }
}
