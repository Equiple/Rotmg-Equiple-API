using RotmgleWebApi.Games;

namespace RotmgleWebApi.Players
{
    public class PlayerProfile
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Role { get; set; }

        public DateAndTime RegistrationDate { get; set; }

        public GameStatisticDetailed NormalStats { get; set; }

        public GameStatisticDetailed DailyStats { get; set; }

        public int DailyGuesses { get; set; }
    }
}
