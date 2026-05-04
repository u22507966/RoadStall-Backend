using System.Text.Json.Serialization;

namespace RoadStallAPI.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public int SaleGroup { get; set; }
        public int StockId { get; set; }
        public int QuantitySold { get; set; }
        public decimal TotalPrice { get; set; }
        public required DateTime Date { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Stock? Stock { get; set; }
    }
}
