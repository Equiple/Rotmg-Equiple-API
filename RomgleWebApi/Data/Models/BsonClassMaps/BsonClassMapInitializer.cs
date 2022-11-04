using RomgleWebApi.Data.Models.Auth;
using RomgleWebApi.Utils;

namespace RomgleWebApi.Data.Models.BsonClassMaps
{
    public static class BsonClassMapInitializer
    {
        public static void Initialize()
        {
            BsonDateAndTimeMap.Register();
            BsonDailyMap.Register();
            BsonClassMapUtils.RegisterIdOnly<Player>(player => player.Id);
            BsonClassMapUtils.RegisterIdOnly<Item>(item => item.Id);
            BsonClassMapUtils.RegisterIdOnly<RefreshToken>(token => token.Token);
        }
    }
}
