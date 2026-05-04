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
    public class RolesController : ControllerBase
    {
        private readonly RoadStallDbContext _context;

        public RolesController(RoadStallDbContext context)
        {
            _context = context;
        }

        // GET: api/Roles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetRole()
        {
            return await _context.Role.ToListAsync();
        }

        // GET: api/Roles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRole(int id)
        {
            var role = await _context.Role.FindAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            return role;
        }

        // GET: api/Roles/by-name/{roleName}
        [HttpGet("by-name/{roleName}")]
        public async Task<ActionResult<Role>> GetRoleByName(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return BadRequest(new { message = "Role name is required" });
            }

            var role = await _context.Role
                .FirstOrDefaultAsync(r => r.RoleName == roleName);

            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }

            return role;
        }

        // GET: api/Roles/5/users
        [HttpGet("{id}/users")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsersByRole(int id)
        {
            var role = await _context.Role.FindAsync(id);
            
            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }

            var users = await _context.User
                .Where(u => u.RoleId == id)
                .ToListAsync();

            return users;
        }

        // PUT: api/Roles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRole(int id, Role role)
        {
            if (id != role.Id)
            {
                return BadRequest(new { message = "Role ID mismatch" });
            }

            // Check if role name already exists (excluding current role)
            var existingRole = await _context.Role
                .FirstOrDefaultAsync(r => r.RoleName == role.RoleName && r.Id != id);

            if (existingRole != null)
            {
                return BadRequest(new { message = "Role name already exists" });
            }

            _context.Entry(role).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoleExists(id))
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

        // POST: api/Roles
        [HttpPost]
        public async Task<ActionResult<Role>> PostRole(Role role)
        {
            try
            {
                // Check if role name already exists
                var existingRole = await _context.Role
                    .FirstOrDefaultAsync(r => r.RoleName == role.RoleName);

                if (existingRole != null)
                {
                    return BadRequest(new { message = "Role name already exists" });
                }

                _context.Role.Add(role);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetRole", new { id = role.Id }, role);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to create role", error = ex.Message });
            }
        }

        // DELETE: api/Roles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _context.Role.FindAsync(id);
            
            if (role == null)
            {
                return NotFound();
            }

            // Check if any users are assigned to this role
            var usersWithRole = await _context.User
                .Where(u => u.RoleId == id)
                .CountAsync();

            if (usersWithRole > 0)
            {
                return BadRequest(new { 
                    message = $"Cannot delete role. {usersWithRole} user(s) are assigned to this role.",
                    usersCount = usersWithRole 
                });
            }

            _context.Role.Remove(role);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RoleExists(int id)
        {
            return _context.Role.Any(e => e.Id == id);
        }
    }
}
