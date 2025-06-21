using ExpenseTrackerAPI.Models;

namespace ExpenseTrackerAPI.DTO
{
    public class NewExpenseRequest
    {
        public string Description { get; set; } = string.Empty;
        public Category Category { get; set; } = Category.Others;
        public decimal Amount { get; set; } = 0;
        public DateTime? ExpenseTime { get; set; } = DateTime.UtcNow;
        
    }
}
