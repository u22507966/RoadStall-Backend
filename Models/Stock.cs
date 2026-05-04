using System.Text.Json.Serialization;

namespace RoadStallAPI.Models
{
    public class Stock
    {
        public int Id { get; set; }
        public required string StockName { get; set; }
        public int Quantity { get; set; } = 0;
        public decimal Price { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<StockTake>? StockTakes { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<StockChange>? StockChanges { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<Sale>? Sales { get; set; }
    }
}
