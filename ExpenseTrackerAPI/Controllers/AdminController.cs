using ExpenseTrackerAPI.Data;
using ExpenseTrackerAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTrackerAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly ExpenseTrackerDBContext _context;

        public AdminController(ExpenseTrackerDBContext context)
        {
            _context = context;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new
                    {
                        u.Id,
                        u.Username,
                        u.Role,
                        u.CreatedAt
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex) 
            {
                return StatusCode(500, new { message = "An error occurred while fetching users.", error = ex.Message });
            }
        }

        [HttpGet("/users/id")]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == id);

                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                return Ok(new
                {
                    user.Id,
                    user.Username,
                    user.Role,
                    user.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching users.", error = ex.Message });
            }
        }

        [HttpDelete("/users/id")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == id);

                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return Ok("User deleted successfully");

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching users.", error = ex.Message });
            }
        }

        [HttpPut("/users/id")]
        public async Task<IActionResult> MakeUserAdmin(string id)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == id);

                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                user.Role = "Admin";

                await _context.SaveChangesAsync();

                return Ok(new { message = "Expense updated successfully.", user.Username, user.Id, user.Role });
            }
            catch (Exception ex) 
            {
                return StatusCode(500, new { message = "An error occurred while fetching users.", error = ex.Message });
            }
        }

        [HttpGet("expenses")]
        public async Task<IActionResult> GetExpensesOfUser(string id)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == id);

                if(user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                var expenses = await _context.Expenses
                    .Where(e => e.UserId == user.Id)
                    .Select(e => new
                    {
                        e.Id,
                        e.Description,
                        e.Amount,
                        e.ExpenseTime,
                        e.Category
                    }).ToListAsync();

                return Ok(expenses);
                    
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching expenses.", error = ex.Message });
            }
        }

        [HttpGet("summary")]
        public async Task<IActionResult> Summary()
        {
            try
            {
                int totalUsers = await _context.Users.CountAsync();
                int totalExpenses = await _context.Expenses.CountAsync();
                decimal totalAmountSpent = await _context.Expenses.SumAsync(e => e.Amount);
                var topCategories = await _context.Expenses
                    .GroupBy(e => e.Category)
                    .Select(g => new
                    {
                        Category = g.Key.ToString(),
                        TotalAmount = g.Sum(e => e.Amount)
                    })
                    .OrderByDescending(g => g.TotalAmount)
                    .Take(2)
                    .ToListAsync();

                return Ok(new
                {
                    TotalUsers = totalUsers,
                    TotalExpenses = totalExpenses,
                    TotalAmountSpent = totalAmountSpent,
                    TopCategories = topCategories
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching summary.", error = ex.Message });
            }
        }

        [HttpGet("summary/user/id")]
        public async Task<IActionResult> SummaryByUser(string userId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);

                if(user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                int totalExpenses = await _context.Expenses.CountAsync(e => e.UserId == user.Id);
                decimal totalAmountSpent = await _context.Expenses
                    .Where(e => e.UserId == user.Id)
                    .SumAsync(e => e.Amount);

                var topCategories = await _context.Expenses
                    .Where(e => e.UserId == user.Id)
                    .GroupBy(e => e.Category)
                    .Select(g => new
                    {
                        Category = g.Key.ToString(),
                        TotalAmount = g.Sum(e => e.Amount)
                    })
                    .OrderByDescending(g => g.TotalAmount)
                    .Take(2)
                    .ToListAsync();

                return Ok(new
                {
                    User = user.Username,
                    TotalExpenses = totalExpenses,
                    TotalAmountSpent = totalAmountSpent,
                    TopCategories = topCategories
                });
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching summary by user.", error = ex.Message });
            }
        }


    }
}
