namespace RoadStallAPI.Models.DTOs
{
    public class StockTakeHistoryDto
    {
        public int StockId { get; set; }
        public string StockName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public DateTime SnapshotDate { get; set; }
        public int OpeningStock { get; set; }
        public int ClosingStock { get; set; }
        public int Variance => ClosingStock - OpeningStock;
    }
}
