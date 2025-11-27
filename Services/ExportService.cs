using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using BudgetApp.Models;
using Microsoft.Win32;

namespace BudgetApp.Services
{
    public class ExportService
    {
        public void ExportToCsv(List<Transaction> transactions, string? filePath = null)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    var saveDialog = new SaveFileDialog
                    {
                        Filter = "CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*",
                        DefaultExt = "csv",
                        FileName = $"Транзакции_{DateTime.Now:yyyy-MM-dd}.csv"
                    };

                    if (saveDialog.ShowDialog() != true)
                    {
                        return;
                    }

                    filePath = saveDialog.FileName;
                }

                var csv = new StringBuilder();
                csv.AppendLine("Дата;Тип;Категория;Описание;Сумма");

                foreach (var transaction in transactions.OrderByDescending(t => t.Date))
                {
                    var type = transaction.Type == TransactionType.Income ? "Доход" : "Расход";
                    var line = $"{transaction.Date:dd.MM.yyyy};{type};{transaction.Category};" +
                              $"{transaction.Description};{transaction.Amount.ToString("F2", CultureInfo.InvariantCulture)}";
                    csv.AppendLine(line);
                }

                File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
                MessageBox.Show($"Данные успешно экспортированы в файл:\n{filePath}", 
                    "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ExportSummaryToCsv(BudgetService budgetService, DataService dataService, 
            DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*",
                    DefaultExt = "csv",
                    FileName = $"Отчет_{DateTime.Now:yyyy-MM-dd}.csv"
                };

                if (saveDialog.ShowDialog() != true)
                {
                    return;
                }

                var csv = new StringBuilder();
                csv.AppendLine("ОТЧЕТ О БЮДЖЕТЕ");
                csv.AppendLine($"Период: {startDate?.ToString("dd.MM.yyyy") ?? "Все время"} - {endDate?.ToString("dd.MM.yyyy") ?? "Все время"}");
                csv.AppendLine();

                var income = budgetService.GetTotalIncome(startDate, endDate);
                var expenses = budgetService.GetTotalExpenses(startDate, endDate);
                var balance = budgetService.GetBalance(startDate, endDate);

                csv.AppendLine("ОБЩАЯ СТАТИСТИКА");
                csv.AppendLine($"Доходы;{income.ToString("F2", CultureInfo.InvariantCulture)}");
                csv.AppendLine($"Расходы;{expenses.ToString("F2", CultureInfo.InvariantCulture)}");
                csv.AppendLine($"Баланс;{balance.ToString("F2", CultureInfo.InvariantCulture)}");
                csv.AppendLine();

                csv.AppendLine("РАСХОДЫ ПО КАТЕГОРИЯМ");
                csv.AppendLine("Категория;Сумма");
                var expenseCategories = budgetService.GetExpensesByCategory(startDate, endDate);
                foreach (var kvp in expenseCategories.OrderByDescending(x => x.Value))
                {
                    csv.AppendLine($"{kvp.Key};{kvp.Value.ToString("F2", CultureInfo.InvariantCulture)}");
                }
                csv.AppendLine();

                csv.AppendLine("ДОХОДЫ ПО КАТЕГОРИЯМ");
                csv.AppendLine("Категория;Сумма");
                var incomeCategories = budgetService.GetIncomeByCategory(startDate, endDate);
                foreach (var kvp in incomeCategories.OrderByDescending(x => x.Value))
                {
                    csv.AppendLine($"{kvp.Key};{kvp.Value.ToString("F2", CultureInfo.InvariantCulture)}");
                }

                File.WriteAllText(saveDialog.FileName, csv.ToString(), Encoding.UTF8);
                MessageBox.Show($"Отчет успешно экспортирован в файл:\n{saveDialog.FileName}", 
                    "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

