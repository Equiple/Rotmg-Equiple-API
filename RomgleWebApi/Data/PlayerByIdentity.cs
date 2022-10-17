using RomgleWebApi.Data.Models;
using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.Data
{
    public readonly struct PlayerByIdentity
    {
        public PlayerByIdentity(Player player, Identity identity)
        {
            Player = player;
            Identity = identity;
        }

        public Player Player { get; }

        public Identity Identity { get; }
    }
}
