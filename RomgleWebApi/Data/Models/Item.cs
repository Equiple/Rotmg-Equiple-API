using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RomgleWebApi.Data.Extensions;

namespace RomgleWebApi.Data.Models
{
    public class Item
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; } = "";
        public string ImageURL { get; set; } = "";
        public string? XmlId { get; set; }
        public string Tier { get; set; } = "Tiered";
        public bool Reskin { get; set; } = false;
        public string Type { get; set; } = "";
        //public string damageRange { get; set; } = "0";
        public int DamageBottom { get; set; } = 0;
        public int? DamageTop { get; set; }
        public double? Range { get; set; } = 0;
        public int? NumberOfShots { get; set; } = 0;
        public int XpBonus { get; set; } = 0;
        public int Feedpower { get; set; } = 0;
        public string Tags { get; set; } = "";
        public string DominantColor { get; set; } = "";
        public string ColorClass { get; set; } = "";
    }
}
