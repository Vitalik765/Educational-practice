namespace BudgetApp.Models
{
    public class Category
    {
        public string Name { get; set; } = string.Empty;
        public TransactionType Type { get; set; }
        public string Color { get; set; } = "#FF6B6B";
    }
}

