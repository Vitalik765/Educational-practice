using BudgetApp.Models;

namespace BudgetApp.Models
{
    public class BudgetLimitViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public TransactionType Type { get; set; }
        public decimal Limit { get; set; }
        public string Period { get; set; } = "Month";
        public decimal Used { get; set; }
        public decimal Remaining => Limit - Used;
        public bool IsOverLimit => Used > Limit;
        public BudgetLimit OriginalLimit { get; set; } = null!;
    }
}

