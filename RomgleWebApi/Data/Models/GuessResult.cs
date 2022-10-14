namespace RomgleWebApi.Data.Models
{
    public class GuessResult
    {
        public GuessStatus Status { get; set; }

        public Hints Hints { get; set; }

        public Item TargetItem { get; set; }
    }
}
