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
        /// Creates a snapshot of ALL stocks in the system and rolls stock takes forward for the next day. Also sets stock.stockLeft = closing stock before rollover
        /// - Stocks updated today: Real opening/closing values         ------ THIS IS NOW BROKEN/IRRELEVANT, rollover means that all products are considered "updated" and will have opening = previous closing, closing = 0
        /// - Stocks NOT updated today: Opening/Closing = 0
        /// </summary>
        [HttpPost("snapshot")]
        public async Task<IActionResult> CreateDailySnapshot()
        {
            try
            {
                var today = DateTime.Today;
                var nextDay = today.AddDays(1);

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
                        Price = stock.Price,
                        StockLeft = stock.Quantity
                    };
                }).ToList();

                _context.StockTakeHistory.AddRange(historyRecords);
                await _context.SaveChangesAsync();


                //Rollover code to set opening = closing, and then closing = 0
                var stockTakesToRollOver = await _context.StockTake.ToListAsync();

                foreach (var stockTake in stockTakesToRollOver)
                {
                    stockTake.OpeningStock = stockTake.ClosingStock;
                    
                    var thisStock = await _context.Stock.FindAsync(stockTake.StockId);          //Updating stock quantity to closing stock, otherwise there is disparity between opening stock and stock left values the next day
                    if(thisStock == null)
                    {
                        continue; // Skip if stock not found
                    }
                    thisStock.Quantity = stockTake.ClosingStock;

                    stockTake.ClosingStock = 0;
                    stockTake.Date = nextDay;
                }

                await _context.SaveChangesAsync();

                var updatedCount = todaysStockTakes.Count;
                var notUpdatedCount = allStocks.Count - updatedCount;

                //Setting 

                return Ok(new 
                { 
                    message = "Daily snapshot created successfully and stock takes rolled over for the next day",
                    date = today,
                    nextBusinessDate = nextDay,
                    totalProducts = allStocks.Count,
                    productsUpdatedToday = updatedCount,
                    productsNotUpdated = notUpdatedCount,
                    stockTakesRolledOver = stockTakesToRollOver.Count,
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
                    ClosingStock = h.ClosingStock,
                    StockLeft = h.StockLeft
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
                    ClosingStock = h.ClosingStock,
                    StockLeft = h.StockLeft
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
                    ClosingStock = h.ClosingStock,
                    StockLeft = h.StockLeft
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
                .OrderBy(h => h.StockId)
                .ToListAsync();

            if (!history.Any())
            {
                return NotFound(new { message = $"No history found for {date:yyyy-MM-dd}" });
            }

            var stockChanges = await _context.StockChange
                .Include(sc => sc.User)
                .Where(sc => sc.ChangeDate.Date == date.Date &&
                    (sc.ChangeType == "Stock Received" || sc.ChangeType == "Stock Removed"))
                .OrderBy(sc => sc.StockId)
                .ThenBy(sc => sc.ChangeDate)
                .ThenBy(sc => sc.Id)
                .Select(sc => new
                {
                    sc.StockId,
                    Id = sc.Id,
                    Quantity = sc.Quantity,
                    ChangeDate = sc.ChangeDate,
                    UserId = sc.UserId,
                    Username = sc.User != null ? sc.User.Username : "Unknown",
                    ChangeType = sc.ChangeType
                })
                .ToListAsync();

            var receivedByStockId = stockChanges
                .Where(sc => sc.ChangeType == "Stock Received")
                .GroupBy(sc => sc.StockId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(sc => new StockChangeExportItemDto
                    {
                        Id = sc.Id,
                        Quantity = sc.Quantity,
                        ChangeDate = sc.ChangeDate,
                        UserId = sc.UserId,
                        Username = sc.Username,
                        ChangeType = sc.ChangeType
                    }).ToList());

            var removedByStockId = stockChanges
                .Where(sc => sc.ChangeType == "Stock Removed")
                .GroupBy(sc => sc.StockId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(sc => new StockChangeExportItemDto
                    {
                        Id = sc.Id,
                        Quantity = sc.Quantity,
                        ChangeDate = sc.ChangeDate,
                        UserId = sc.UserId,
                        Username = sc.Username,
                        ChangeType = sc.ChangeType
                    }).ToList());

            // Get ACTUAL SALES from the Sale table for this date
            var unitsSold = await _context.Sale
                .Where(s => s.Date.Date == date.Date)
                .GroupBy(s => s.StockId)
                .Select(g => new { StockId = g.Key, TotalSold = g.Sum(s => s.QuantitySold) })
                .ToDictionaryAsync(x => x.StockId, x => x.TotalSold);

            var exportData = history.Select(h =>
            {
                var receivedEntries = receivedByStockId.TryGetValue(h.StockId, out var received)
                    ? received
                    : [];
                var removedEntries = removedByStockId.TryGetValue(h.StockId, out var removed)
                    ? removed
                    : [];
                var totalReceived = receivedEntries.Sum(entry => entry.Quantity);
                var totalRemoved = removedEntries.Sum(entry => entry.Quantity);

                return new StockTakeHistoryExportDto
                {
                    Date = h.SnapshotDate.ToString("yyyy-MM-dd"),
                    StockId = h.StockId,
                    StockName = h.StockName,
                    Price = h.Price,
                    OpeningStock = h.OpeningStock,
                    StockReceived = totalReceived,
                    StockRemoved = totalRemoved,
                    UnitsSold = unitsSold.ContainsKey(h.StockId) ? unitsSold[h.StockId] : 0,
                    ClosingStock = h.ClosingStock,
                    StockLeft = h.StockLeft,
                    Variance = h.ClosingStock - h.OpeningStock,
                    ExpectedClosing = h.OpeningStock + totalReceived - totalRemoved - (unitsSold.ContainsKey(h.StockId) ? unitsSold[h.StockId] : 0),
                    RecordedBy = h.User != null ? h.User.Username : "system",
                    WasUpdatedToday = h.OpeningStock > 0 || h.ClosingStock > 0,
                    ReceivedEntries = receivedEntries,
                    RemovedEntries = removedEntries,
                    StockReceivedDisplay = string.Join(Environment.NewLine, receivedEntries.Select(entry => $"+{entry.Quantity}")),
                    StockRemovedDisplay = string.Join(Environment.NewLine, removedEntries.Select(entry => $"-{entry.Quantity}"))
                };
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
