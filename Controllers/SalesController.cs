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
    public class SalesController : ControllerBase
    {
        private readonly RoadStallDbContext _context;

        public SalesController(RoadStallDbContext context)
        {
            _context = context;
        }

        // GET: api/Sales
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sale>>> GetSale()
        {
            var sales = await _context.Sale.ToListAsync();
            return sales;
        }

        // GET: api/Sales/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Sale>> GetSale(int id)
        {
            var sale = await _context.Sale.FindAsync(id);

            if (sale == null)
            {
                return NotFound();
            }

            return sale;
        }

        [HttpGet("exportSales/{day}")]
        public async Task<ActionResult<List<Sale>>> GetSalesFromDay(DateTime day)
        {
            var start = day.Date;
            var end = start.AddDays(1);

            if(day.Date > DateTime.Today)
            {
                return BadRequest("Cannot ask for future dates");
            }
      
            
            var sales = await _context.Sale.Where(s => s.Date >= start && s.Date < end).ToListAsync();

            if (sales.Count <= 0)
            {
                return BadRequest("No sales were made on the specified date");
            }

            return sales;


        }

        // PUT: api/Sales/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSale(int id, Sale sale)
        {
            if (id != sale.Id)
            {
                return BadRequest();
            }

            //minusing the sale from stock.quantity
            var stock = await _context.Stock.FindAsync(sale.StockId);
            if(stock == null)
            {
                return BadRequest("Couldnt find the stock for this sale");
            }
            stock.Quantity = stock.Quantity - sale.QuantitySold;

            _context.Entry(sale).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SaleExists(id))
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

        // POST: api/Sales
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Sale>> PostSale(Sale sale)
        {
            sale.TotalPrice = sale.QuantitySold * sale.TotalPrice;
            sale.Date = DateTime.Now;

            var stock = await _context.Stock.FindAsync(sale.StockId);
            if (stock == null)
            {
                return BadRequest("Could not find the stock for this sale");
            }
            stock.Quantity -= sale.QuantitySold;

            _context.Sale.Add(sale);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSale", new { id = sale.Id }, sale);
        }

        // DELETE: api/Sales/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSale(int id)
        {
            var sale = await _context.Sale.FindAsync(id);
            if (sale == null)
            {
                return NotFound();
            }

            _context.Sale.Remove(sale);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SaleExists(int id)
        {
            return _context.Sale.Any(e => e.Id == id);
        }

        [HttpGet("unitsSold/{id}")]
        public async Task<ActionResult<int>> GetUnitsSold(int id)
        {
            var today = DateTime.Today;
            int totalUnitsSold = await _context.Sale
                .Where(s => s.Date.Date == today && s.StockId == id)
                .SumAsync(s => s.QuantitySold);
            
            return totalUnitsSold;
        }

        [HttpGet("getDates")]
        public async Task<ActionResult<DateOnly[]>> getDates()
        {
            var today = DateTime.Today;
            var dates = await _context.Sale.Select(s => s.Date).Where(s => s.Date < today).Distinct().ToListAsync();
            if (dates.Count == 0)
            {
                return NotFound(new {message = "No dates with historical data found"});
            }

            return Ok(dates);
        }
    }
}
