using RotmgleWebApi.Items;

namespace RotmgleWebApi.Games
{
    public class GameOptions
    {
        public Gamemode Mode { get; set; }

        public List<Item> Guesses { get; set; }

        public List<Hints> AllHints { get; set; }

        public string Anagram { get; set; }

        public string Description { get; set; }

        public bool ReskinsExcluded { get; set; }
    }
}
