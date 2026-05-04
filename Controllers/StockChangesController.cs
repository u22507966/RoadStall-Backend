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
    public class StockChangesController : ControllerBase
    {
        private readonly RoadStallDbContext _context;

        public StockChangesController(RoadStallDbContext context)
        {
            _context = context;
        }

        // GET: api/StockChanges
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockChange>>> GetStockChange()
        {
            return await _context.StockChange.ToListAsync();
        }

        // GET: api/StockChanges/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StockChange>> GetStockChange(int id)
        {
            var stockChange = await _context.StockChange.FindAsync(id);

            if (stockChange == null)
            {
                return NotFound();
            }

            return stockChange;
        }

        // PUT: api/StockChanges/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStockChange(int id, StockChange stockChange)
        {
            if (id != stockChange.Id)
            {
                return BadRequest();
            }

            _context.Entry(stockChange).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StockChangeExists(id))
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

        // POST: api/StockChanges
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<StockChange>> PostStockChange(StockChange stockChange)
        {
            stockChange.ChangeDate = DateTime.Now;
            _context.StockChange.Add(stockChange);

            //need to delete/add quantity froim stock.quantity
            var stock = await _context.Stock.FindAsync(stockChange.StockId);
            if (stock == null)
            {
                return BadRequest();
            }
            if(stockChange.ChangeType == "Stock Removed")
            {
                stock.Quantity -= stockChange.Quantity;
            }
            else
            {
                stock.Quantity += stockChange.Quantity;
            }

            await _context.SaveChangesAsync();
            return CreatedAtAction("GetStockChange", new { id = stockChange.Id }, stockChange);
        }

        // DELETE: api/StockChanges/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStockChange(int id)
        {
            var stockChange = await _context.StockChange.FindAsync(id);
            if (stockChange == null)
            {
                return NotFound();
            }

            _context.StockChange.Remove(stockChange);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StockChangeExists(int id)
        {
            return _context.StockChange.Any(e => e.Id == id);
        }
    }
}
