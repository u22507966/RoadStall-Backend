using System.Text.Json.Serialization;

namespace RoadStallAPI.Models
{
    public class StockTakeHistory
    {
        public int Id { get; set; }
        public int StockId { get; set; }
        public int UserId { get; set; }
        public DateTime SnapshotDate { get; set; }
        public int OpeningStock { get; set; }
        public int ClosingStock { get; set; }
        public required string StockName { get; set; }
        public decimal Price { get; set; }

        public int StockLeft { get; set; }

        // Navigation properties
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Stock? Stock { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public User? User { get; set; }
    }
}
