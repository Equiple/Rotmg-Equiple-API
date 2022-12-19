namespace RomgleWebApi.Data.Models
{
    public class GameOptions
    {
        public Gamemode Mode { get; set; }

        public IReadOnlyList<Item> Guesses { get; set; } 

        public IReadOnlyList<Hints> AllHints { get; set; }
        
        public string Anagram { get; set; }

        public string Description { get; set; }

        public bool ReskinsExcluded { get; set; }
    }
}
