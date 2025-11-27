using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BudgetApp.Models;
using BudgetApp.Services;

namespace BudgetApp.Windows
{
    public partial class CategoryEditWindow : Window
    {
        private readonly DataService _dataService;
        private readonly Category? _category;

        public CategoryEditWindow(DataService dataService, Category? category = null)
        {
            InitializeComponent();
            _dataService = dataService;
            _category = category;

            if (category != null)
            {
                Title = "Редактировать категорию";
                NameTextBox.Text = category.Name;
                
                var typeItem = TypeComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Tag?.ToString() == category.Type.ToString());
                if (typeItem != null)
                {
                    TypeComboBox.SelectedItem = typeItem;
                }

                var colorItem = ColorComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Tag?.ToString() == category.Color);
                if (colorItem != null)
                {
                    ColorComboBox.SelectedItem = colorItem;
                }
                else
                {
                    ColorComboBox.SelectedIndex = 0;
                }
            }
            else
            {
                Title = "Добавить категорию";
                TypeComboBox.SelectedIndex = 1; // Расход по умолчанию
                ColorComboBox.SelectedIndex = 0;
            }

            UpdateColorPreview();
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateColorPreview();
        }

        private void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateColorPreview();
        }

        private void UpdateColorPreview()
        {
            if (ColorComboBox.SelectedItem is ComboBoxItem selectedItem && 
                selectedItem.Tag is string colorHex)
            {
                var brush = (SolidColorBrush?)new BrushConverter().ConvertFrom(colorHex);
                if (brush != null)
                {
                    ColorPreview.Fill = brush;
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Введите название категории", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (TypeComboBox.SelectedItem is not ComboBoxItem selectedType)
            {
                return;
            }

            if (ColorComboBox.SelectedItem is not ComboBoxItem selectedColor)
            {
                return;
            }

            var category = _category ?? new Category();
            var oldName = _category?.Name;
            var oldType = _category?.Type ?? TransactionType.Expense;

            category.Name = NameTextBox.Text.Trim();
            category.Type = Enum.Parse<TransactionType>(selectedType.Tag?.ToString() ?? "Expense");
            category.Color = selectedColor.Tag?.ToString() ?? "#FF6B6B";

            // Проверяем, не существует ли уже категория с таким именем и типом
            var existingCategory = _dataService.GetCategories()
                .FirstOrDefault(c => c.Name == category.Name && c.Type == category.Type);
            
            if (existingCategory != null && (oldName != category.Name || oldType != category.Type))
            {
                MessageBox.Show("Категория с таким названием и типом уже существует", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Если редактируем существующую категорию
            if (_category != null && oldName != null)
            {
                // Если изменили название или тип, обновляем транзакции и удаляем старую категорию
                if (oldName != category.Name || oldType != category.Type)
                {
                    var transactions = _dataService.GetTransactions()
                        .Where(t => t.Category == oldName && t.Type == oldType)
                        .ToList();

                    foreach (var transaction in transactions)
                    {
                        transaction.Category = category.Name;
                        transaction.Type = category.Type;
                        _dataService.UpdateTransaction(transaction);
                    }

                    _dataService.DeleteCategory(oldName, oldType);
                }
                else
                {
                    // Просто обновляем цвет - удаляем и добавляем заново
                    _dataService.DeleteCategory(oldName, oldType);
                }
            }

            _dataService.AddCategory(category);
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

