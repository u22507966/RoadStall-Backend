namespace RoadStallAPI.Models.DTOs
{
    public class StockTakeHistoryExportDto
    {
        public string Date { get; set; } = string.Empty;
        public int StockId { get; set; }
        public string StockName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int OpeningStock { get; set; }
        public int StockReceived { get; set; }
        public int StockRemoved { get; set; }
        public int UnitsSold { get; set; }
        public int ClosingStock { get; set; }
        public int StockLeft { get; set; }
        public int Variance { get; set; }
        public int ExpectedClosing { get; set; }
        public string RecordedBy { get; set; } = string.Empty;
        public bool WasUpdatedToday { get; set; }
        public List<StockChangeExportItemDto> ReceivedEntries { get; set; } = [];
        public List<StockChangeExportItemDto> RemovedEntries { get; set; } = [];
        public string StockReceivedDisplay { get; set; } = string.Empty;
        public string StockRemovedDisplay { get; set; } = string.Empty;
    }

    public class StockChangeExportItemDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public DateTime ChangeDate { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty;
    }
}
