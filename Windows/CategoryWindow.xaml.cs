using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BudgetApp.Models;
using BudgetApp.Services;
using Microsoft.Win32;

namespace BudgetApp.Windows
{
    public partial class CategoryWindow : Window
    {
        private readonly DataService _dataService;
        private bool _isInitialized = false;

        public CategoryWindow(DataService dataService)
        {
            InitializeComponent();
            _dataService = dataService;
            _isInitialized = true;
            LoadCategories();
        }

        private void LoadCategories()
        {
            if (_dataService == null) return;
            
            var categories = _dataService.GetCategories();

            if (TypeFilterComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var filter = selectedItem.Tag?.ToString();
                if (filter == "Income")
                {
                    categories = categories.Where(c => c.Type == TransactionType.Income).ToList();
                }
                else if (filter == "Expense")
                {
                    categories = categories.Where(c => c.Type == TransactionType.Expense).ToList();
                }
            }

            CategoriesGrid.ItemsSource = categories;
        }

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CategoryEditWindow(_dataService, null);
            if (dialog.ShowDialog() == true)
            {
                LoadCategories();
            }
        }

        private void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Category category)
            {
                var dialog = new CategoryEditWindow(_dataService, category);
                if (dialog.ShowDialog() == true)
                {
                    LoadCategories();
                }
            }
        }

        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Category category)
            {
                // Проверяем, используется ли категория в транзакциях
                var transactions = _dataService.GetTransactions();
                var isUsed = transactions.Any(t => t.Category == category.Name && t.Type == category.Type);

                if (isUsed)
                {
                    MessageBox.Show(
                        "Эта категория используется в транзакциях. Сначала удалите или измените все транзакции с этой категорией.",
                        "Невозможно удалить",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить категорию '{category.Name}'?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _dataService.DeleteCategory(category.Name, category.Type);
                    LoadCategories();
                }
            }
        }

        private void TypeFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Не загружаем категории во время инициализации
            if (!_isInitialized) return;
            
            LoadCategories();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

