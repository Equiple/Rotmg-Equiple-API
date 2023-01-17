using RomgleWebApi.Data;
using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.Utils
{
    public static class PlayerUtils
    {
        public static NewPlayer Create(
            Identity identity,
            string deviceId,
            string personalKey,
            string personalKeyEncoding = "ASCII")
        {
            Device device = DeviceUtils.Create(
                deviceId,
                personalKey,
                personalKeyEncoding: personalKeyEncoding);
            Player player = new Player
            {
                Name = identity.Details.Name,
                Role = "user",
                RegistrationDate = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow,
                Identities = new List<Identity> { identity },
                Devices = new List<Device> { device },
                NormalStats = new GameStatistic(),
                DailyStats = new GameStatistic(),
                EndedGames = new List<Game>()
            };
            return new NewPlayer(player, device);
        }
    }
}
