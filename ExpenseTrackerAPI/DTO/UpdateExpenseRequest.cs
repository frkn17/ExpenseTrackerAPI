using ExpenseTrackerAPI.Models;

namespace ExpenseTrackerAPI.DTO
{
    public class UpdateExpenseRequest
    {
        public string Description { get; set; } = string.Empty;
        public Category Category { get; set; } = Category.Others;
        public decimal Amount { get; set; } = decimal.Zero;
        public DateTime ExpenseTime { get; set; } = DateTime.Now;
    }
}
