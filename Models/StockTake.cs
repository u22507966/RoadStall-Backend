using System.Text.Json.Serialization;

namespace RoadStallAPI.Models
{
    public class StockTake
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public User? User { get; set; }
        
        public int StockId { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Stock? Stock { get; set; }
        
        public DateTime Date { get; set; }
        public int OpeningStock { get; set; }
        public int ClosingStock { get; set; }
    }
}
