namespace BudgetApp.Models
{
    public class BudgetLimit
    {
        public string CategoryName { get; set; } = string.Empty;
        public TransactionType Type { get; set; }
        public decimal Limit { get; set; }
        public string Period { get; set; } = "Month"; // Month, Week, Year
    }
}

