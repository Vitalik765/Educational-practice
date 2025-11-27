using System;
using System.Linq;
using System.Windows;
using BudgetApp.Services;

namespace BudgetApp.Windows
{
    public partial class ReportsWindow : Window
    {
        private readonly BudgetService _budgetService;
        private readonly DataService _dataService;
        private readonly ExportService _exportService;

        public ReportsWindow(BudgetService budgetService, DataService dataService)
        {
            InitializeComponent();
            _budgetService = budgetService;
            _dataService = dataService;
            _exportService = new ExportService();

            StartDatePicker.SelectedDate = DateTime.Now.AddMonths(-1);
            EndDatePicker.SelectedDate = DateTime.Now;

            LoadReports();
        }

        private void LoadReports()
        {
            var startDate = StartDatePicker.SelectedDate;
            var endDate = EndDatePicker.SelectedDate;

            var income = _budgetService.GetTotalIncome(startDate, endDate);
            var expenses = _budgetService.GetTotalExpenses(startDate, endDate);
            var balance = _budgetService.GetBalance(startDate, endDate);

            PeriodIncomeText.Text = $"{income:N2} ₽";
            PeriodExpenseText.Text = $"{expenses:N2} ₽";
            PeriodBalanceText.Text = $"{balance:N2} ₽";

            var expenseCategories = _budgetService.GetExpensesByCategory(startDate, endDate);
            var totalExpenses = expenses > 0 ? expenses : 1;
            var expenseList = expenseCategories.Select(kvp => new
            {
                Key = kvp.Key,
                Value = kvp.Value,
                Percentage = (kvp.Value / totalExpenses) * 100
            }).OrderByDescending(x => x.Value).ToList();

            ExpenseReportList.ItemsSource = expenseList;

            var incomeCategories = _budgetService.GetIncomeByCategory(startDate, endDate);
            var totalIncome = income > 0 ? income : 1;
            var incomeList = incomeCategories.Select(kvp => new
            {
                Key = kvp.Key,
                Value = kvp.Value,
                Percentage = (kvp.Value / totalIncome) * 100
            }).OrderByDescending(x => x.Value).ToList();

            IncomeReportList.ItemsSource = incomeList;
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            LoadReports();
        }

        private void ExportReport_Click(object sender, RoutedEventArgs e)
        {
            _exportService.ExportSummaryToCsv(_budgetService, _dataService, 
                StartDatePicker.SelectedDate, EndDatePicker.SelectedDate);
        }
    }
}

