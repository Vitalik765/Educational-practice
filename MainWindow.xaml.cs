using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BudgetApp.Models;
using BudgetApp.Services;
using BudgetApp.Windows;

namespace BudgetApp
{
    public partial class MainWindow : Window
    {
        private readonly DataService _dataService;
        private readonly BudgetService _budgetService;
        private readonly ExportService _exportService;
        private readonly User _currentUser;

        public MainWindow(DataService dataService, User currentUser)
        {
            InitializeComponent();
            _dataService = dataService;
            _budgetService = new BudgetService(_dataService);
            _exportService = new ExportService();
            _currentUser = currentUser;

            Title = $"Управление бюджетом - Полнофункциональное приложение ({_currentUser.Username} | {_currentUser.Role})";
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Загружаем данные после полной инициализации UI
                _dataService.EnsureDataLoaded();
                
                LoadData();
                LoadCategoryFilter();
                ConfigureAccessByRole();
                
                // Инициализация поиска после полной загрузки окна
                if (SearchTextBox != null)
                {
                    SearchTextBox.Text = "Поиск по описанию...";
                    SearchTextBox.Foreground = new SolidColorBrush(Colors.Gray);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при инициализации приложения:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ConfigureAccessByRole()
        {
            bool isAdmin = _currentUser.Role == UserRole.Admin;

            AddTransactionButton.IsEnabled = isAdmin;
            ManageCategoriesButton.IsEnabled = isAdmin;
            ManageLimitsButton.IsEnabled = isAdmin;
            ExportButton.IsEnabled = isAdmin;

            if (ActionsColumn != null)
            {
                ActionsColumn.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void LoadData()
        {
            RefreshTransactions();
            UpdateStatistics();
            UpdateCategoryStatistics();
        }

        private void RefreshTransactions()
        {
            if (_dataService == null) return;
            
            var transactions = _dataService.GetTransactions().AsQueryable();

            // Фильтр по типу
            if (FilterComboBox.SelectedItem is ComboBoxItem filterItem)
            {
                var filterText = filterItem.Content.ToString();
                if (filterText == "Доходы")
                {
                    transactions = transactions.Where(t => t.Type == TransactionType.Income);
                }
                else if (filterText == "Расходы")
                {
                    transactions = transactions.Where(t => t.Type == TransactionType.Expense);
                }
            }

            // Фильтр по категории
            if (CategoryFilterComboBox.SelectedItem is ComboBoxItem categoryItem && 
                categoryItem.Content.ToString() != "Все категории")
            {
                var categoryName = categoryItem.Content.ToString();
                transactions = transactions.Where(t => t.Category == categoryName);
            }

            // Поиск
            if (!string.IsNullOrWhiteSpace(SearchTextBox.Text) && 
                SearchTextBox.Text != "Поиск по описанию...")
            {
                var searchText = SearchTextBox.Text.ToLower();
                transactions = transactions.Where(t => t.Description.ToLower().Contains(searchText));
            }

            TransactionsGrid.ItemsSource = transactions.OrderByDescending(t => t.Date).ToList();
        }

        private void LoadCategoryFilter()
        {
            if (_dataService == null) return;
            
            var categories = _dataService.GetCategories()
                .OrderBy(c => c.Name)
                .Select(c => c.Name)
                .Distinct()
                .ToList();

            CategoryFilterComboBox.Items.Clear();
            CategoryFilterComboBox.Items.Add(new ComboBoxItem { Content = "Все категории", IsSelected = true });

            foreach (var category in categories)
            {
                CategoryFilterComboBox.Items.Add(new ComboBoxItem { Content = category });
            }
        }

        private void UpdateStatistics()
        {
            if (_budgetService == null) return;
            
            var totalIncome = _budgetService.GetTotalIncome();
            var totalExpenses = _budgetService.GetTotalExpenses();
            var balance = _budgetService.GetBalance();

            TotalIncomeText.Text = $"{totalIncome:N2} ₽";
            TotalExpenseText.Text = $"{totalExpenses:N2} ₽";
            BalanceText.Text = $"{balance:N2} ₽";
            BalanceText.Foreground = balance >= 0 
                ? System.Windows.Media.Brushes.White 
                : System.Windows.Media.Brushes.IndianRed;
        }

        private void UpdateCategoryStatistics()
        {
            if (_budgetService == null || _dataService == null) return;
            
            var expenseCategories = _budgetService.GetExpensesByCategory();
            var incomeCategories = _budgetService.GetIncomeByCategory();
            var allCategories = _dataService.GetCategories();

            var expenseList = expenseCategories.Select(kvp =>
            {
                var category = allCategories.FirstOrDefault(c => c.Name == kvp.Key && c.Type == TransactionType.Expense);
                return new
                {
                    Key = kvp.Key,
                    Value = kvp.Value,
                    Color = category?.Color ?? "#FF6B6B"
                };
            }).OrderByDescending(x => x.Value).ToList();

            var incomeList = incomeCategories.Select(kvp =>
            {
                var category = allCategories.FirstOrDefault(c => c.Name == kvp.Key && c.Type == TransactionType.Income);
                return new
                {
                    Key = kvp.Key,
                    Value = kvp.Value,
                    Color = category?.Color ?? "#4ECDC4"
                };
            }).OrderByDescending(x => x.Value).ToList();

            ExpenseCategoriesList.ItemsSource = expenseList;
            IncomeCategoriesList.ItemsSource = incomeList;
        }

        private void AddTransaction_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser.Role != UserRole.Admin)
            {
                MessageBox.Show("Добавление транзакций доступно только администраторам.", "Нет доступа",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_dataService == null) return;
            
            // Убеждаемся, что данные загружены
            _dataService.EnsureDataLoaded();
            
            var dialog = new TransactionWindow(_dataService, null);
            if (dialog.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void EditTransaction_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser.Role != UserRole.Admin)
            {
                MessageBox.Show("Редактирование транзакций доступно только администраторам.", "Нет доступа",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_dataService == null) return;
            
            // Убеждаемся, что данные загружены
            _dataService.EnsureDataLoaded();
            
            if (sender is Button button && button.Tag is int id)
            {
                var transaction = _dataService.GetTransactions().FirstOrDefault(t => t.Id == id);
                if (transaction != null)
                {
                    var dialog = new TransactionWindow(_dataService, transaction);
                    if (dialog.ShowDialog() == true)
                    {
                        LoadData();
                    }
                }
            }
        }

        private void DeleteTransaction_Click(object sender, RoutedEventArgs e)
        {
            if (_dataService == null) return;
            if (_currentUser.Role != UserRole.Admin)
            {
                MessageBox.Show("Удаление транзакций доступно только администраторам.", "Нет доступа",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (sender is Button button && button.Tag is int id)
            {
                var result = MessageBox.Show(
                    "Вы уверены, что хотите удалить эту транзакцию?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _dataService.DeleteTransaction(id);
                    LoadData();
                }
            }
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshTransactions();
        }

        private void CategoryFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshTransactions();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshTransactions();
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == "Поиск по описанию...")
            {
                SearchTextBox.Text = "";
                SearchTextBox.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SearchTextBox.Text = "Поиск по описанию...";
                SearchTextBox.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        private void ManageCategories_Click(object sender, RoutedEventArgs e)
        {
            if (_dataService == null) return;
            
            if (_currentUser.Role != UserRole.Admin)
            {
                MessageBox.Show("Управление категориями доступно только администраторам.", "Нет доступа",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // Убеждаемся, что данные загружены
            _dataService.EnsureDataLoaded();
            
            var categoryWindow = new CategoryWindow(_dataService);
            categoryWindow.ShowDialog();
            LoadData();
            LoadCategoryFilter();
        }

        private void ExportData_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser.Role != UserRole.Admin)
            {
                MessageBox.Show("Экспорт доступен только администраторам.", "Нет доступа",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_dataService == null || _exportService == null) return;
            
            var transactions = _dataService.GetTransactions();
            _exportService.ExportToCsv(transactions);
        }

        private void ManageLimits_Click(object sender, RoutedEventArgs e)
        {
            if (_dataService == null || _budgetService == null) return;
            
            if (_currentUser.Role != UserRole.Admin)
            {
                MessageBox.Show("Управление лимитами доступно только администраторам.", "Нет доступа",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var limitsWindow = new BudgetLimitsWindow(_dataService, _budgetService);
            limitsWindow.ShowDialog();
        }

        private void ShowReports_Click(object sender, RoutedEventArgs e)
        {
            if (_budgetService == null || _dataService == null) return;
            
            var reportsWindow = new ReportsWindow(_budgetService, _dataService);
            reportsWindow.ShowDialog();
        }
    }
}

