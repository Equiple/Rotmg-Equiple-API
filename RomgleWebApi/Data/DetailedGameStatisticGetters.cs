using RomgleWebApi.Data.Models;

namespace RomgleWebApi.Data
{
    public struct DetailedGameStatisticGetters
    {
        public Func<string, Task<Item>>? BestGuessItem { get; set; }
    }
}
