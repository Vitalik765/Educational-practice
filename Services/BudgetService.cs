using System;
using System.Collections.Generic;
using System.Linq;
using BudgetApp.Models;

namespace BudgetApp.Services
{
    public class BudgetService
    {
        private readonly DataService _dataService;

        public BudgetService(DataService dataService)
        {
            _dataService = dataService;
        }

        public decimal GetTotalIncome(DateTime? startDate = null, DateTime? endDate = null)
        {
            var transactions = GetFilteredTransactions(startDate, endDate);
            return transactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);
        }

        public decimal GetTotalExpenses(DateTime? startDate = null, DateTime? endDate = null)
        {
            var transactions = GetFilteredTransactions(startDate, endDate);
            return transactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);
        }

        public decimal GetBalance(DateTime? startDate = null, DateTime? endDate = null)
        {
            return GetTotalIncome(startDate, endDate) - GetTotalExpenses(startDate, endDate);
        }

        public Dictionary<string, decimal> GetExpensesByCategory(DateTime? startDate = null, DateTime? endDate = null)
        {
            var transactions = GetFilteredTransactions(startDate, endDate)
                .Where(t => t.Type == TransactionType.Expense);

            return transactions
                .GroupBy(t => t.Category)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
        }

        public Dictionary<string, decimal> GetIncomeByCategory(DateTime? startDate = null, DateTime? endDate = null)
        {
            var transactions = GetFilteredTransactions(startDate, endDate)
                .Where(t => t.Type == TransactionType.Income);

            return transactions
                .GroupBy(t => t.Category)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
        }

        private List<Transaction> GetFilteredTransactions(DateTime? startDate, DateTime? endDate)
        {
            var transactions = _dataService.GetTransactions();
            
            if (startDate.HasValue)
            {
                transactions = transactions.Where(t => t.Date >= startDate.Value).ToList();
            }
            
            if (endDate.HasValue)
            {
                transactions = transactions.Where(t => t.Date <= endDate.Value).ToList();
            }

            return transactions;
        }
    }
}

