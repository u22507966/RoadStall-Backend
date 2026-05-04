using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoadStallAPI.Models;
using RoadStallAPI.Models.DTOs;

namespace RoadStallAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockTakeHistoryController : ControllerBase
    {
        private readonly RoadStallDbContext _context;

        public StockTakeHistoryController(RoadStallDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a snapshot of ALL stocks in the system - Called by Azure Function at midnight
        /// - Stocks updated today: Real opening/closing values
        /// - Stocks NOT updated today: Opening/Closing = 0
        /// </summary>
        [HttpPost("snapshot")]
        public async Task<IActionResult> CreateDailySnapshot()
        {
            try
            {
                var today = DateTime.Today;

                // Check if snapshot already exists for today
                var existingSnapshot = await _context.StockTakeHistory
                    .AnyAsync(h => h.SnapshotDate.Date == today);

                if (existingSnapshot)
                {
                    return BadRequest(new { message = $"Snapshot for {today:yyyy-MM-dd} already exists" });
                }

                // Get ALL stocks in the system
                var allStocks = await _context.Stock.ToListAsync();

                if (!allStocks.Any())
                {
                    return Ok(new 
                    { 
                        message = "No stocks in the system. No snapshot created.", 
                        date = today,
                        count = 0 
                    });
                }

                // Get a default user (first active user, or first user if none active)
                var defaultUser = await _context.User.FirstOrDefaultAsync(u => u.Status == 1) 
                    ?? await _context.User.FirstOrDefaultAsync();

                if (defaultUser == null)
                {
                    return BadRequest(new { message = "No users exist in the system. Please create at least one user before taking snapshots." });
                }

                // Get stock takes that were updated TODAY
                var todaysStockTakes = await _context.StockTake
                    .Include(st => st.User)
                    .Where(st => st.Date.Date == today)
                    .ToDictionaryAsync(st => st.StockId);

                // Create history records for ALL stocks
                var historyRecords = allStocks.Select(stock =>
                {
                    var stockTake = todaysStockTakes.ContainsKey(stock.Id) 
                        ? todaysStockTakes[stock.Id] 
                        : null;

                    return new StockTakeHistory
                    {
                        StockId = stock.Id,
                        UserId = stockTake?.UserId ?? defaultUser.Id, // Use actual default user
                        SnapshotDate = today,
                        OpeningStock = stockTake?.OpeningStock ?? 0,  // 0 if not updated today
                        ClosingStock = stockTake?.ClosingStock ?? 0,  // 0 if not updated today
                        StockName = stock.StockName,
                        Price = stock.Price
                    };
                }).ToList();

                _context.StockTakeHistory.AddRange(historyRecords);
                await _context.SaveChangesAsync();

                var updatedCount = todaysStockTakes.Count;
                var notUpdatedCount = allStocks.Count - updatedCount;

                return Ok(new 
                { 
                    message = "Daily snapshot created successfully for all products",
                    date = today,
                    totalProducts = allStocks.Count,
                    productsUpdatedToday = updatedCount,
                    productsNotUpdated = notUpdatedCount,
                    defaultUserId = defaultUser.Id,
                    defaultUsername = defaultUser.Username,
                    updatedStocks = todaysStockTakes.Values.Select(st => 
                        allStocks.First(s => s.Id == st.StockId).StockName).ToList(),
                    notUpdatedStocks = allStocks
                        .Where(s => !todaysStockTakes.ContainsKey(s.Id))
                        .Select(s => s.StockName)
                        .ToList()
                });
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database error creating snapshot: {dbEx.Message}");
                Console.WriteLine($"Inner exception: {dbEx.InnerException?.Message}");
                return StatusCode(500, new { 
                    message = "Failed to create snapshot", 
                    error = dbEx.Message,
                    innerError = dbEx.InnerException?.Message,
                    details = "Check that all foreign key constraints are satisfied (users must exist)"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating snapshot: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Failed to create snapshot", error = ex.Message });
            }
        }

        /// <summary>
        /// Get history for a specific date
        /// </summary>
        [HttpGet("by-date/{date}")]
        public async Task<ActionResult<IEnumerable<StockTakeHistoryDto>>> GetHistoryByDate(DateTime date)
        {
            var history = await _context.StockTakeHistory
                .Include(h => h.Stock)
                .Include(h => h.User)
                .Where(h => h.SnapshotDate.Date == date.Date)
                .Select(h => new StockTakeHistoryDto
                {
                    StockId = h.StockId,
                    StockName = h.StockName,
                    Price = h.Price,
                    UserId = h.UserId,
                    Username = h.User != null ? h.User.Username : "Unknown",
                    SnapshotDate = h.SnapshotDate,
                    OpeningStock = h.OpeningStock,
                    ClosingStock = h.ClosingStock
                })
                .ToListAsync();

            return Ok(history);
        }

        /// <summary>
        /// Get history for a date range
        /// </summary>
        [HttpGet("range")]
        public async Task<ActionResult<IEnumerable<StockTakeHistoryDto>>> GetHistoryRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
            {
                return BadRequest(new { message = "Start date must be before end date" });
            }

            var history = await _context.StockTakeHistory
                .Include(h => h.Stock)
                .Include(h => h.User)
                .Where(h => h.SnapshotDate.Date >= startDate.Date && h.SnapshotDate.Date <= endDate.Date)
                .OrderByDescending(h => h.SnapshotDate)
                .ThenBy(h => h.StockName)
                .Select(h => new StockTakeHistoryDto
                {
                    StockId = h.StockId,
                    StockName = h.StockName,
                    Price = h.Price,
                    UserId = h.UserId,
                    Username = h.User != null ? h.User.Username : "Unknown",
                    SnapshotDate = h.SnapshotDate,
                    OpeningStock = h.OpeningStock,
                    ClosingStock = h.ClosingStock
                })
                .ToListAsync();

            return Ok(history);
        }

        /// <summary>
        /// Get history for a specific stock
        /// </summary>
        [HttpGet("by-stock/{stockId}")]
        public async Task<ActionResult<IEnumerable<StockTakeHistoryDto>>> GetHistoryByStock(int stockId)
        {
            var history = await _context.StockTakeHistory
                .Include(h => h.User)
                .Where(h => h.StockId == stockId)
                .OrderByDescending(h => h.SnapshotDate)
                .Select(h => new StockTakeHistoryDto
                {
                    StockId = h.StockId,
                    StockName = h.StockName,
                    Price = h.Price,
                    UserId = h.UserId,
                    Username = h.User != null ? h.User.Username : "Unknown",
                    SnapshotDate = h.SnapshotDate,
                    OpeningStock = h.OpeningStock,
                    ClosingStock = h.ClosingStock
                })
                .ToListAsync();

            return Ok(history);
        }

        /// <summary>
        /// Get all available snapshot dates
        /// </summary>
        [HttpGet("dates")]
        public async Task<ActionResult<IEnumerable<DateTime>>> GetAvailableDates()
        {
            var dates = await _context.StockTakeHistory
                .Select(h => h.SnapshotDate.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToListAsync();

            return Ok(dates);
        }

        /// <summary>
        /// Get Excel export data for a specific date - Returns data formatted for Excel
        /// Includes stock received, removed, and ACTUAL SALES from the Sale table
        /// </summary>
        [HttpGet("export/{date}")]
        public async Task<ActionResult<object>> GetExportData(DateTime date)
        {
            var history = await _context.StockTakeHistory
                .Include(h => h.User)
                .Where(h => h.SnapshotDate.Date == date.Date)
                .OrderBy(h => h.StockName)
                .ToListAsync();

            if (!history.Any())
            {
                return NotFound(new { message = $"No history found for {date:yyyy-MM-dd}" });
            }

            // Get stock received for this date
            var stockReceived = await _context.StockChange
                .Where(sc => sc.ChangeDate.Date == date.Date && sc.ChangeType == "Stock Received")
                .GroupBy(sc => sc.StockId)
                .Select(g => new { StockId = g.Key, TotalReceived = g.Sum(sc => sc.Quantity) })
                .ToDictionaryAsync(x => x.StockId, x => x.TotalReceived);

            // Get stock removed for this date
            var stockRemoved = await _context.StockChange
                .Where(sc => sc.ChangeDate.Date == date.Date && sc.ChangeType == "Stock Removed")
                .GroupBy(sc => sc.StockId)
                .Select(g => new { StockId = g.Key, TotalRemoved = g.Sum(sc => sc.Quantity) })
                .ToDictionaryAsync(x => x.StockId, x => x.TotalRemoved);

            // Get ACTUAL SALES from the Sale table for this date
            var unitsSold = await _context.Sale
                .Where(s => s.Date.Date == date.Date)
                .GroupBy(s => s.StockId)
                .Select(g => new { StockId = g.Key, TotalSold = g.Sum(s => s.QuantitySold) })
                .ToDictionaryAsync(x => x.StockId, x => x.TotalSold);

            var exportData = history.Select(h => new
            {
                Date = h.SnapshotDate.ToString("yyyy-MM-dd"),
                StockName = h.StockName,
                Price = h.Price,
                OpeningStock = h.OpeningStock,
                StockReceived = stockReceived.ContainsKey(h.StockId) ? stockReceived[h.StockId] : 0,
                StockRemoved = stockRemoved.ContainsKey(h.StockId) ? stockRemoved[h.StockId] : 0,
                UnitsSold = unitsSold.ContainsKey(h.StockId) ? unitsSold[h.StockId] : 0,  // ACTUAL SALES
                ClosingStock = h.ClosingStock,
                Variance = h.ClosingStock - h.OpeningStock,
                // Calculate expected closing stock to show discrepancies
                ExpectedClosing = h.OpeningStock 
                    + (stockReceived.ContainsKey(h.StockId) ? stockReceived[h.StockId] : 0)
                    - (stockRemoved.ContainsKey(h.StockId) ? stockRemoved[h.StockId] : 0)
                    - (unitsSold.ContainsKey(h.StockId) ? unitsSold[h.StockId] : 0),
                RecordedBy = h.User != null ? h.User.Username : "system",
                WasUpdatedToday = h.OpeningStock > 0 || h.ClosingStock > 0
            }).ToList();

            return Ok(new
            {
                date = date.ToString("yyyy-MM-dd"),
                totalRecords = exportData.Count,
                activeProducts = exportData.Count(x => x.WasUpdatedToday),
                inactiveProducts = exportData.Count(x => !x.WasUpdatedToday),
                data = exportData
            });
        }

        /// <summary>
        /// Delete old history records (optional cleanup)
        /// </summary>
        [HttpDelete("cleanup")]
        public async Task<IActionResult> CleanupOldRecords([FromQuery] int daysToKeep = 90)
        {
            var cutoffDate = DateTime.Today.AddDays(-daysToKeep);

            var oldRecords = await _context.StockTakeHistory
                .Where(h => h.SnapshotDate < cutoffDate)
                .ToListAsync();

            if (!oldRecords.Any())
            {
                return Ok(new { message = "No old records to delete", deletedCount = 0 });
            }

            _context.StockTakeHistory.RemoveRange(oldRecords);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Deleted records older than {daysToKeep} days",
                deletedCount = oldRecords.Count,
                cutoffDate = cutoffDate
            });
        }
    }
}
