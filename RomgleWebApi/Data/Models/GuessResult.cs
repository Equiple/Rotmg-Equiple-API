namespace RomgleWebApi.Data.Models
{
    public class GuessResult
    {
        public GuessStatus Status { get; set; }

        public Hints Hints { get; set; } = null;

        public Item? targetItem { get; set; } = null;

    }
}
