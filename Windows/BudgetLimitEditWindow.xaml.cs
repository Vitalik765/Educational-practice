using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BudgetApp.Models;
using BudgetApp.Services;

namespace BudgetApp.Windows
{
    public partial class BudgetLimitEditWindow : Window
    {
        private readonly DataService _dataService;
        private readonly BudgetLimit? _limit;

        public BudgetLimitEditWindow(DataService dataService, BudgetLimit? limit = null)
        {
            InitializeComponent();
            _dataService = dataService;
            _limit = limit;

            TypeComboBox.SelectedIndex = 1; // Расход по умолчанию
            LoadCategories();

            if (limit != null)
            {
                Title = "Редактировать лимит";
                LimitTextBox.Text = limit.Limit.ToString(CultureInfo.InvariantCulture);

                var typeItem = TypeComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Tag?.ToString() == limit.Type.ToString());
                if (typeItem != null)
                {
                    TypeComboBox.SelectedItem = typeItem;
                }

                var periodItem = PeriodComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Tag?.ToString() == limit.Period);
                if (periodItem != null)
                {
                    PeriodComboBox.SelectedItem = periodItem;
                }

                LoadCategories();
                var selectedCategory = CategoryComboBox.Items.Cast<Category>()
                    .FirstOrDefault(c => c.Name == limit.CategoryName && c.Type == limit.Type);
                if (selectedCategory != null)
                {
                    CategoryComboBox.SelectedItem = selectedCategory;
                }
            }
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadCategories();
        }

        private void LoadCategories()
        {
            if (TypeComboBox.SelectedItem is ComboBoxItem selectedType)
            {
                var type = System.Enum.Parse<TransactionType>(selectedType.Tag?.ToString() ?? "Expense");
                var categories = _dataService.GetCategories()
                    .Where(c => c.Type == type)
                    .ToList();

                CategoryComboBox.ItemsSource = categories;
                CategoryComboBox.DisplayMemberPath = "Name";

                if (categories.Any() && CategoryComboBox.SelectedItem == null)
                {
                    CategoryComboBox.SelectedIndex = 0;
                }
            }
        }

        private void LimitTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex(@"^[0-9]+(\.[0-9]{0,2})?$");
            var text = (sender as TextBox)?.Text + e.Text;
            e.Handled = !regex.IsMatch(text);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (CategoryComboBox.SelectedItem is not Category category)
            {
                MessageBox.Show("Выберите категорию", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(LimitTextBox.Text) || 
                !decimal.TryParse(LimitTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var limit))
            {
                MessageBox.Show("Введите корректный лимит", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (TypeComboBox.SelectedItem is not ComboBoxItem selectedType)
            {
                return;
            }

            if (PeriodComboBox.SelectedItem is not ComboBoxItem selectedPeriod)
            {
                return;
            }

            var budgetLimit = _limit ?? new BudgetLimit();
            budgetLimit.CategoryName = category.Name;
            budgetLimit.Type = System.Enum.Parse<TransactionType>(selectedType.Tag?.ToString() ?? "Expense");
            budgetLimit.Limit = limit;
            budgetLimit.Period = selectedPeriod.Tag?.ToString() ?? "Month";

            _dataService.AddBudgetLimit(budgetLimit);
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

