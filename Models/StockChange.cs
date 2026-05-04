using System.Text.Json.Serialization;

namespace RoadStallAPI.Models
{
    public class StockChange
    {
        public int Id { get; set; }
        public int StockId { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Stock? Stock { get; set; }
        
        public int UserId { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public User? User { get; set; }
        
        public required string ChangeType { get; set; }
        public int Quantity { get; set; }
        public DateTime ChangeDate { get; set; }

        public int? Adjustment { get; set; } = 0;
    }
}
