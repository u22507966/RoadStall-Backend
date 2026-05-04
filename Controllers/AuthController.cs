using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoadStallAPI.Models;
using RoadStallAPI.Models.DTOs;
using RoadStallAPI.Services;

namespace RoadStallAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RoadStallDbContext _context;
        private readonly IAuthService _authService;

        public AuthController(RoadStallDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username and password are required" });
            }

            var user = await _context.User
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            if (!_authService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            if (user.Status != 1)
            {
                return Unauthorized(new { message = "Account is not active. Please contact administrator." });
            }

            var token = _authService.GenerateToken(user.Id, user.Username);

            var response = new LoginResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Phone = user.Phone,
                Status = user.Status,
                Token = token
            };

            return Ok(response);
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new { message = "Username and password are required" });
                }

                // Check if username already exists
                var existingUser = await _context.User
                    .FirstOrDefaultAsync(u => u.Username == request.Username || 
                        (!string.IsNullOrEmpty(request.Email) && u.Email == request.Email));

                if (existingUser != null)
                {
                    return BadRequest(new { message = "Username or email already exists" });
                }

                var passwordHash = _authService.HashPassword(request.Password);

                var newUser = new User
                {
                    Username = request.Username,
                    PasswordHash = passwordHash,
                    Email = request.Email,
                    Phone = request.Phone,
                    Status = 0 // Inactive by default - admin needs to approve
                };

                _context.User.Add(newUser);
                await _context.SaveChangesAsync();

                var token = _authService.GenerateToken(newUser.Id, newUser.Username);

                var response = new LoginResponse
                {
                    UserId = newUser.Id,
                    Username = newUser.Username,
                    Email = newUser.Email,
                    Phone = newUser.Phone,
                    Status = newUser.Status,
                    Token = token
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Registration failed", error = ex.Message });
            }
        }
    }
}
