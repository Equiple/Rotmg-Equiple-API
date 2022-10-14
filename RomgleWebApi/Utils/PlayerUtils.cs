using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.Utils
{
    public static class PlayerUtils
    {
        public static Player Create(Identity identity, string secretKey)
        {
            return new Player
            {
                Name = identity.Details.Name,
                RegistrationDate = DateTime.UtcNow,
                Identities = new List<Identity> { identity },
                SecretKey = secretKey,
                RefreshTokens = new List<RefreshToken>(),
                NormalStats = new GameStatistic(),
                DailyStats = new GameStatistic(),
                EndedGames = new List<Game>()
            };
        }
    }
}
