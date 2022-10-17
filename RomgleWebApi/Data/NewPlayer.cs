using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.Data
{
    public readonly struct NewPlayer
    {
        public NewPlayer(Player player, Device device)
        {
            Player = player;
            Device = device;
        }

        public Player Player { get; }

        public Device Device { get; }
    }
}
