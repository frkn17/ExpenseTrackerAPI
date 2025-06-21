namespace ExpenseTrackerAPI.Models
{
    public class Expense
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Description { get; set; } = string.Empty;
        public Category Category { get; set; } = Category.Others;
        public decimal Amount { get; set; } = 0;

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpenseTime { get; set; } = DateTime.UtcNow;

        public User User { get; set; }

    }
}
