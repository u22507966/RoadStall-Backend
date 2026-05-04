using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using RoadStallAPI;
using RoadStallAPI.Models;
using RoadStallAPI.Models.DTOs;
using RoadStallAPI.Services;

namespace RoadStallAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly RoadStallDbContext _context;
        private readonly IAuthService _authService;

        public UsersController(RoadStallDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserListDto>>> GetUser()
        {
            var users = await _context.User
                .Select(u => new UserListDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Status = u.Status,
                    RoleId = u.RoleId
                })
                .ToListAsync();

            return users;
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.User.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        [HttpGet("by-username/{username}")]
        public async Task<ActionResult<int>> GetUserId(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(new { message = "Username is not valid" });
            }
            var user = await _context.User.FirstOrDefaultAsync(u => u.Username == username);
            if(user == null)
            {
                return BadRequest(new { Message = "User not found" });
            }
            return Ok(user.Id);
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, [FromBody] UpdateUserRequest request)
        {
            var user = await _context.User.FindAsync(id);
            
            if (user == null)
            {
                return NotFound();
            }

            // Update only the fields that should be changed
            user.Username = request.Username;
            user.Email = request.Email;
            user.Phone = request.Phone;
            user.Status = request.Status;
            // PasswordHash is NOT updated here - keeps existing password

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

        // PATCH: api/Users/5/status
        // Simple endpoint to update just the user status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] int status)
        {
            var user = await _context.User.FindAsync(id);
            
            if (user == null)
            {
                return NotFound();
            }

            user.Status = status;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] int roleId)
        {
            var user = await _context.User.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.RoleId = roleId;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            _context.User.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.User.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.Id == id);
        }




        //Getting users roleId
        [HttpGet("{Id}/role")]
        public async Task<ActionResult<int>> GetRoleId(int Id)
        {
            if(Id < 0)
            {
                return BadRequest();
            }
            var user = await _context.User.FindAsync(Id);
            if(user == null)
            {
                return NotFound();
            }

            return Ok(user.RoleId);
            
        }
    }
}
