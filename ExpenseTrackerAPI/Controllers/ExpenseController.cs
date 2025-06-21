using ExpenseTrackerAPI.Data;
using ExpenseTrackerAPI.DTO;
using ExpenseTrackerAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ExpenseTrackerAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ExpenseController : ControllerBase
    {
        private readonly ExpenseTrackerDBContext _context;

        public ExpenseController(ExpenseTrackerDBContext context)
        {
            _context = context;
        }

        [HttpPost("newExpense")]
        public async Task<IActionResult> AddNewExpense([FromBody] NewExpenseRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (!checkIfAuthorized())
                {
                    return Unauthorized(new { message = "User not authenticated." });
                }

                Guid userId = Guid.Parse(userIdClaim.Value);

                var newExpense = new Expense
                {
                    UserId = userId,
                    Description = request.Description,
                    Category = request.Category,
                    Amount = request.Amount,
                    ExpenseTime = request.ExpenseTime ?? DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Expenses.AddAsync(newExpense);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Expense added successfully.",
                    expense = new
                    {
                        newExpense.Id,
                        newExpense.Description,
                        newExpense.Category,
                        newExpense.Amount,
                        newExpense.ExpenseTime,
                        newExpense.CreatedAt
                    }
                });
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpGet("getExpenses")]
        public async Task<IActionResult> GetExpenses([FromQuery] int? filter, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (!checkIfAuthorized())
                {
                    return Unauthorized(new { message = "User not authenticated." });
                }

                Guid userId = Guid.Parse(userIdClaim!.Value);
                DateTime fromDate = DateTime.MinValue;
                DateTime toDate = DateTime.UtcNow;

                switch (filter)
                {
                    case 0: //Past week
                        fromDate = DateTime.UtcNow.AddDays(-7);
                        break;
                    case 1: //Past month
                        fromDate = DateTime.UtcNow.AddMonths(-1);
                        break;
                    case 2: //Last 3 months
                        fromDate = DateTime.UtcNow.AddMonths(-3);
                        break;
                    case 3: //Custom
                        if (startDate == null || endDate == null)
                            return BadRequest(new { message = "Start date and end date are required for custom filter." });

                        fromDate = startDate.Value;
                        endDate = endDate.Value;
                        break;
                    default:
                        fromDate = DateTime.MinValue;
                        break;
                }

                var expenses = await _context.Expenses
                    .Where(e => e.UserId == userId && e.ExpenseTime >= fromDate && e.ExpenseTime <= toDate)
                    .OrderByDescending(e => e.ExpenseTime)
                    .ToListAsync();

                return Ok(expenses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpDelete("id")]
        public async Task<IActionResult> DeleteExpense(Guid id)
        {
            try
            {
                var userClaimId = User.FindFirst(ClaimTypes.NameIdentifier);
                if (!checkIfAuthorized())
                {
                    return Unauthorized(new { message = "User not authenticated." });
                }

                var expense = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id);

                if (expense == null)
                {
                    return NotFound(new { message = "Expense not found or access denied." });
                }

                _context.Expenses.Remove(expense);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Expense deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpDelete("delete-all")]
        public async Task<IActionResult> DeleteAllExpenses()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (!checkIfAuthorized())
                {
                    return Unauthorized(new { message = "User not authenticated." });
                }

                var userId = Guid.Parse(userIdClaim!.Value);

                var userExpenses = _context.Expenses.Where(e => e.UserId == userId);

                _context.Expenses.RemoveRange(userExpenses);
                await _context.SaveChangesAsync();

                return Ok(new { message = "All expenses deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpPut("id")]
        public async Task<IActionResult> UpdateExpense(Guid id, [FromBody] UpdateExpenseRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (!checkIfAuthorized())
                {
                    return Unauthorized(new { message = "User not authenticated." });
                }

                var expense = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id);

                if (expense == null)
                    return NotFound(new { message = "Expense not found or access denied." });

                expense.Description = request.Description;
                expense.Amount = request.Amount;
                expense.ExpenseTime = request.ExpenseTime;
                expense.Category = request.Category;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Expense updated successfully." });
            }
            catch (Exception ex) 

            {
                return StatusCode(500, new { message = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            try
            {
                if(!checkIfAuthorized())
                {
                    return Unauthorized(new { message = "User not authenticated." });
                }

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var summary = await _context.Expenses
                    .Where(e => e.UserId == userId)
                    .GroupBy(e => e.Category)
                    .Select(g => new
                    {
                        Category = g.Key.ToString(),
                        TotalAmount = g.Sum(e => e.Amount),
                        Count = g.Count()
                    })
                    .OrderByDescending(g => g.TotalAmount)
                    .ToListAsync();

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpGet("getExpenseById/id")]
        public async Task<IActionResult> GetExpenseById(Guid id)
        {
            try
            {
                if(!checkIfAuthorized())
                {
                    return Unauthorized(new { message = "User not authenticated." });
                }

                
                var expense = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id);

                if (expense == null)
                {
                    return NotFound(new { message = "Expense not found or access denied." });
                }

                return Ok(expense);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpGet("summary/monthly")]
        public async Task<IActionResult> GetMonthlyExpenes()
        {
            try
            {
                if (!checkIfAuthorized())
                    return Unauthorized(new { message = "User not authenticated." });

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var monthlyStats = await _context.Expenses
                    .Where(e => e.UserId == userId)
                    .GroupBy(e => new { e.ExpenseTime!.Value.Year, e.ExpenseTime!.Value.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        TotalAmount = g.Sum(e => e.Amount),
                    })
                    .OrderBy(g => g.Year).ThenBy(g => g.Month)
                    .ToListAsync();

                return Ok(monthlyStats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred: " + ex.Message });
            }

        }

        private bool checkIfAuthorized()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier) != null;
        }

       
        
    }
}
