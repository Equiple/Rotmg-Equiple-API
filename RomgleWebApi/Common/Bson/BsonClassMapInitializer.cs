﻿using RotmgleWebApi.AuthenticationImplementation;
using RotmgleWebApi.Complaints;
using RotmgleWebApi.Dailies;
using RotmgleWebApi.Items;
using RotmgleWebApi.Players;

namespace RotmgleWebApi
{
    public static class BsonClassMapInitializer
    {
        public static void Initialize()
        {
            DateAndTimeBsonMap.Register();
            BsonDailyMap.Register();
            BsonClassMapUtils.RegisterIdOnly<Player>(player => player.Id);
            BsonClassMapUtils.RegisterIdOnly<Item>(item => item.Id);
            BsonClassMapUtils.RegisterIdOnly<Complaint>(complaint => complaint.Id);

            BsonClassMapUtils.RegisterIdOnly<Session>(session => session.AccessToken);
        }
    }
}
