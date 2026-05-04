using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoadStallAPI;
using RoadStallAPI.Models;

namespace RoadStallAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockTakesController : ControllerBase
    {
        private readonly RoadStallDbContext _context;

        public StockTakesController(RoadStallDbContext context)
        {
            _context = context;
        }

        // GET: api/StockTakes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockTake>>> GetStockTake()
        {
            var stockTakes = await _context.StockTake.ToListAsync();
            var stock = await _context.Stock.ToListAsync();
            var numStocks = stock.Count;

            //first populating missing stock takes, should only happen initially or when missed stock adds and deletes (should sort out adds/dels later on)
            if (stockTakes.Count < stock.Count)
            {
                for (int i = 0; i < stock.Count; i++)
                {
                    for (int j = 0; j < stockTakes.Count; j++)
                    {
                        if (stock[i].Id == stockTakes[j].StockId)
                        {
                            j = stockTakes.Count;
                        }
                        else if (j == stockTakes.Count - 1)
                        {
                            StockTake newStockTake = new StockTake();
                            newStockTake.StockId = stock[i].Id;
                            newStockTake.UserId = 1; // Default user ID, adjust as necessary
                            newStockTake.Date = DateTime.Now;
                            newStockTake.OpeningStock = stock[i].Quantity;
                            newStockTake.ClosingStock = stock[i].Quantity;
                            _context.StockTake.Add(newStockTake);
                            await _context.SaveChangesAsync();
                            stockTakes.Add(newStockTake);
                        }
                    }
                }
            }

            //Secondly populating opening and closing stock based on Stock quantities
            //If opening stock and closing stock are 0, populate opening stock with Stock quantites
            //If opening stock is 0 but closing stock is x, populate opening stock with closing

            //for(int x = 0; x < stockTakes.Count; x++)                                             //I have it commented out because i dont think its actually necessary. If opening and closing are 0 it will be set by user
            //{
            //    var correctStock = await _context.Stock.FindAsync(stockTakes[x].StockId);
            //    if (correctStock == null)
            //    {
            //        continue;
            //    }

            //    if (stockTakes[x].OpeningStock == 0 && stockTakes[x].ClosingStock == 0)
            //    {
            //        stockTakes[x].OpeningStock = correctStock.Quantity;
            //        stockTakes[x].ClosingStock = correctStock.Quantity;
            //        _context.Entry(stockTakes[x]).State = EntityState.Modified;
            //    }
            //    else if (stockTakes[x].OpeningStock == 0 && stockTakes[x].ClosingStock != 0)
            //    {
            //        stockTakes[x].OpeningStock = stockTakes[x].ClosingStock;
            //        _context.Entry(stockTakes[x]).State = EntityState.Modified;
            //    }
            //}

            await _context.SaveChangesAsync();

            return stockTakes;
        }//get method


        // GET: api/StockTakes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StockTake>> GetStockTake(int id)
        {
            var stockTake = await _context.StockTake.FindAsync(id);

            if (stockTake == null)
            {
                return NotFound();
            }

            return stockTake;
        }

        [HttpGet("ByStockId/{id}")]
        public async Task<ActionResult<StockTake>> GetStockTakeByStockId(int id)
        {
            var stockTake = await _context.StockTake
                .FirstOrDefaultAsync(st => st.StockId == id);

            if (stockTake == null)
            {
                return NotFound();
            }

            return stockTake;
        }

        // PUT: api/StockTakes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStockTake(int id, StockTake stockTake)
        {
            if (id != stockTake.Id)
            {
                return BadRequest();
            }

            var stock = await _context.Stock.FindAsync(stockTake.StockId);
            if (stock == null)
            {
                return BadRequest();
            }

            stock.Quantity = await CalculateStockQuantity(stockTake.StockId, stockTake.OpeningStock);

            _context.Entry(stockTake).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StockTakeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/StockTakes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<StockTake>> PostStockTake(StockTake stockTake)
        {
            _context.StockTake.Add(stockTake);

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStockTake", new { id = stockTake.Id }, stockTake);
        }

        // DELETE: api/StockTakes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStockTake(int id)
        {
            var stockTake = await _context.StockTake.FindAsync(id);
            if (stockTake == null)
            {
                return NotFound();
            }

            _context.StockTake.Remove(stockTake);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StockTakeExists(int id)
        {
            return _context.StockTake.Any(e => e.Id == id);
        }

        private async Task<int> CalculateStockQuantity(int stockId, int openingStock)
        {
            var today = DateTime.Today;

            var stockReceived = await _context.StockChange
                .Where(sc => sc.StockId == stockId 
                    && sc.ChangeType == "Stock Received" 
                    && sc.ChangeDate.Date == today)
                .SumAsync(sc => sc.Quantity);

            var stockRemoved = await _context.StockChange
                .Where(sc => sc.StockId == stockId 
                    && sc.ChangeType == "Stock Removed" 
                    && sc.ChangeDate.Date == today)
                .SumAsync(sc => sc.Quantity);

            var totalSales = await _context.Sale
                .Where(s => s.StockId == stockId 
                    && s.Date.Date == today)
                .SumAsync(s => s.QuantitySold);

            return openingStock + stockReceived - stockRemoved - totalSales;
        }
    }
}
