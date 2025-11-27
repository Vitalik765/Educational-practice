using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BudgetApp.Models;
using BudgetApp.Services;

namespace BudgetApp.Windows
{
    public partial class BudgetLimitsWindow : Window
    {
        private readonly DataService _dataService;
        private readonly BudgetService _budgetService;

        public BudgetLimitsWindow(DataService dataService, BudgetService budgetService)
        {
            InitializeComponent();
            _dataService = dataService;
            _budgetService = budgetService;
            LoadLimits();
        }

        private void LoadLimits()
        {
            var limits = _dataService.GetBudgetLimits();
            var transactions = _dataService.GetTransactions();
            var now = DateTime.Now;

            var limitViewModels = limits.Select(limit =>
            {
                DateTime startDate;
                DateTime endDate = now;

                switch (limit.Period)
                {
                    case "Week":
                        startDate = now.AddDays(-(int)now.DayOfWeek);
                        break;
                    case "Month":
                        startDate = new DateTime(now.Year, now.Month, 1);
                        break;
                    case "Year":
                        startDate = new DateTime(now.Year, 1, 1);
                        break;
                    default:
                        startDate = new DateTime(now.Year, now.Month, 1);
                        break;
                }

                var used = transactions
                    .Where(t => t.Category == limit.CategoryName && 
                               t.Type == limit.Type &&
                               t.Date >= startDate && 
                               t.Date <= endDate)
                    .Sum(t => t.Amount);

                return new BudgetLimitViewModel
                {
                    CategoryName = limit.CategoryName,
                    Type = limit.Type,
                    Limit = limit.Limit,
                    Period = limit.Period,
                    Used = used,
                    OriginalLimit = limit
                };
            }).ToList();

            LimitsGrid.ItemsSource = limitViewModels;
        }

        private void AddLimit_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new BudgetLimitEditWindow(_dataService, null);
            if (dialog.ShowDialog() == true)
            {
                LoadLimits();
            }
        }

        private void EditLimit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is BudgetLimitViewModel limitViewModel)
            {
                var dialog = new BudgetLimitEditWindow(_dataService, limitViewModel.OriginalLimit);
                if (dialog.ShowDialog() == true)
                {
                    LoadLimits();
                }
            }
        }

        private void DeleteLimit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is BudgetLimitViewModel limitViewModel)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить лимит для категории '{limitViewModel.CategoryName}'?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _dataService.DeleteBudgetLimit(
                        limitViewModel.CategoryName,
                        limitViewModel.Type,
                        limitViewModel.Period);
                    LoadLimits();
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

