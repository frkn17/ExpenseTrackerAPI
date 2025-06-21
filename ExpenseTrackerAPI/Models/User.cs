namespace ExpenseTrackerAPI.Models
{
    public class User
    {
        public Guid Id { get; set; } = new Guid();
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string Role { get; set; } = "User"; 


        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiryTime {  get; set; } = DateTime.UtcNow;
    }
}
