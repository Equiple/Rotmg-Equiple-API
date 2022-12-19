namespace RomgleWebApi.Data.Models
{
    public class GuessResult
    {
        public GuessStatus Status { get; set; }

        public Item Guess { get; set; }

        public Hints Hints { get; set; }

        public int Tries { get; set; }

        public string Anagram { get; set; }

        public string Description { get; set; }

        public Item TargetItem { get; set; }
    }
}
