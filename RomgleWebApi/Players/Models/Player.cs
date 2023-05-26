using RotmgleWebApi.Authentication;
using RotmgleWebApi.Games;

namespace RotmgleWebApi.Players
{
    public class Player
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Role { get; set; }

        public DateAndTime RegistrationDate { get; set; }

        public DateAndTime LastSeen { get; set; }

        public List<Identity> Identities { get; set; }

        public List<Device> Devices { get; set; }

        public GameStatistic NormalStats { get; set; }

        public GameStatistic DailyStats { get; set; }

        public Game? CurrentGame { get; set; }

        public List<Game> EndedGames { get; set; }
    }
}
