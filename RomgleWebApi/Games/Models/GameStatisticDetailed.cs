using RotmgleWebApi.Items;

namespace RotmgleWebApi.Games
{
    public class GameStatisticDetailed
    {
        public GameStatistic GameStatistic { get; set; }

        public Item? BestGuessItem { get; set; }
    }
}
