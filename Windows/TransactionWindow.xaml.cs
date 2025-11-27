using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using BudgetApp.Models;
using BudgetApp.Services;

namespace BudgetApp.Windows
{
    public partial class TransactionWindow : Window
    {
        private readonly DataService _dataService;
        private readonly Transaction? _transaction;
        private DateTime _selectedDate = DateTime.Now;

        public TransactionWindow(DataService dataService, Transaction? transaction = null)
        {
            InitializeComponent();
            _dataService = dataService;
            _transaction = transaction;

            if (transaction != null)
            {
                Title = "Редактировать транзакцию";
                AmountTextBox.Text = transaction.Amount.ToString(CultureInfo.InvariantCulture);
                DescriptionTextBox.Text = transaction.Description;
                _selectedDate = transaction.Date;
                DatePicker.SelectedDate = transaction.Date;

                var typeItem = TypeComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Tag?.ToString() == transaction.Type.ToString());
                if (typeItem != null)
                {
                    TypeComboBox.SelectedItem = typeItem;
                }
            }
            else
            {
                TypeComboBox.SelectedIndex = 1; // По умолчанию "Расход"
                DatePicker.SelectedDate = DateTime.Now;
                _selectedDate = DateTime.Now;
            }

            LoadCategories();
        }

        private void LoadCategories()
        {
            if (_dataService == null) return;
            
            if (TypeComboBox.SelectedItem is ComboBoxItem selectedType)
            {
                var type = Enum.Parse<TransactionType>(selectedType.Tag?.ToString() ?? "Expense");
                var categories = _dataService.GetCategories()
                    .Where(c => c.Type == type)
                    .ToList();

                CategoryComboBox.ItemsSource = categories;
                CategoryComboBox.DisplayMemberPath = "Name";
                CategoryComboBox.SelectedValuePath = "Name";

                if (_transaction != null)
                {
                    var selectedCategory = categories.FirstOrDefault(c => c.Name == _transaction.Category);
                    if (selectedCategory != null)
                    {
                        CategoryComboBox.SelectedItem = selectedCategory;
                    }
                }
                else if (categories.Any())
                {
                    CategoryComboBox.SelectedIndex = 0;
                }
            }
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadCategories();
        }

        private void AmountTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var regex = new Regex(@"^[0-9]+(\.[0-9]{0,2})?$");
            var text = (sender as TextBox)?.Text + e.Text;
            e.Handled = !regex.IsMatch(text);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AmountTextBox.Text) || 
                !decimal.TryParse(AmountTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
            {
                MessageBox.Show("Введите корректную сумму", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show("Введите описание", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (CategoryComboBox.SelectedItem is not Category category)
            {
                MessageBox.Show("Выберите категорию", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (TypeComboBox.SelectedItem is not ComboBoxItem selectedType)
            {
                return;
            }

            var transaction = _transaction ?? new Transaction();
            transaction.Amount = amount;
            transaction.Description = DescriptionTextBox.Text;
            transaction.Category = category.Name;
            transaction.Type = Enum.Parse<TransactionType>(selectedType.Tag?.ToString() ?? "Expense");
            transaction.Date = _selectedDate;

            if (_transaction == null)
            {
                _dataService.AddTransaction(transaction);
            }
            else
            {
                _dataService.UpdateTransaction(transaction);
            }

            DialogResult = true;
            Close();
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatePicker.SelectedDate.HasValue)
            {
                _selectedDate = DatePicker.SelectedDate.Value;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

