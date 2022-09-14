﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RomgleWebApi.Data.Models
{
    public class Game
    {
        public Gamemode Mode { get; set; }
        public string TargetItemId { get; set; }
        public List<string> GuessItemIds { get; set; }
        public DateTime StartTime { get; set; }
        public bool IsEnded { get; set; } = false;
    }
}