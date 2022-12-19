namespace RomgleWebApi.Data.Models
{
    public class Item
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string ImageURL { get; set; }

        public string? XmlId { get; set; }

        public string Tier { get; set; }

        public string Description { get; set; }

        public bool Reskin { get; set; }

        public string Type { get; set; }

        public int DamageBottom { get; set; }

        public int? DamageTop { get; set; }

        public double? Range { get; set; }

        public int? NumberOfShots { get; set; }

        public int XpBonus { get; set; }

        public int Feedpower { get; set; }

        public List<string> Tags { get; set; }

        public string DominantColor { get; set; }

        public string ColorClass { get; set; }
    }
}
