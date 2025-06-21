using ExpenseTrackerAPI.Data;
using ExpenseTrackerAPI.DTO;
using ExpenseTrackerAPI.Models;
using ExpenseTrackerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ExpenseTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ExpenseTrackerDBContext _context;
        private readonly CreateToken _tokenService;
        
        public UserController(ExpenseTrackerDBContext context, CreateToken tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Signup([FromBody] RegisterRequest request)
        {
            try
            {
                if(String.IsNullOrEmpty(request.Username) || String.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { message = "Username / Password are required." });
                }

                bool userExists = await _context.Users.AnyAsync(u => u.Username == request.Username);
                if (userExists)
                {
                    return BadRequest(new { message = "Username already exists." });
                }

                User newUser = new User
                {
                    Username = request.Username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                };

                await _context.Users.AddAsync(newUser);
                await _context.SaveChangesAsync();

                var token = _tokenService.GenerateJwtToken(newUser);

                return Ok(new
                {
                    message = "User created successfully.",
                    token,
                    user = new
                    {
                        newUser.Id,
                        newUser.Username,
                        newUser.Role,
                        newUser.CreatedAt,
                        newUser.UpdatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred:" + ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == request.Username);

                if(user == null)
                {
                    return BadRequest(new { message = "User not found. Invalid username." });
                }

                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

                if (!isPasswordValid)
                {
                    return BadRequest(new { message = "Invalid password." });
                }

                user.RefreshToken = _tokenService.GenerateRefreshToken();
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(3);

                await _context.SaveChangesAsync();

                var token = _tokenService.GenerateJwtToken(user);

                return Ok(new
                {
                    message = "Login successful.",
                    token,
                    refreshToken = user.RefreshToken,
                    user = new
                    {
                        user.Id,
                        user.Username,
                        user.Role,
                        user.CreatedAt,
                        user.UpdatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred:" + ex.Message });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

                if (user == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
                {
                    return Unauthorized(new { message = "Invalid or expired refresh token" });
                }

                var newAccessToken = _tokenService.GenerateJwtToken(user);
                var newRefreshToken = _tokenService.GenerateRefreshToken();

                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(3);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    token = newAccessToken,
                    refreshToken = newRefreshToken,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred:" + ex.Message });
            }

        }

        
    }
}
