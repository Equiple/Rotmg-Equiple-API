namespace RomgleWebApi.Data.Models
{
    public class DetailedGameStatistic
    {
        public GameStatistic GameStatistic { get; set; }

        public Item? BestGuessItem { get; set; }
    }
}
