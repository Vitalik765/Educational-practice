using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BudgetApp.Models;
using Newtonsoft.Json;

namespace BudgetApp.Services
{
    public class DataService
    {
        private readonly string _dataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BudgetApp",
            "data.json"
        );

        private BudgetData _data = null!;

        private bool _isDataLoaded = false;

        public DataService()
        {
            // Инициализируем только пустые данные в конструкторе
            // Загрузку данных выполним позже, после полной инициализации UI
            _data = new BudgetData();
            InitializeDefaultCategories();
            InitializeDefaultUsers();
        }

        public void EnsureDataLoaded()
        {
            if (_isDataLoaded)
                return;
                
            try
        {
            LoadData();
                _isDataLoaded = true;
            }
            catch
            {
                // Игнорируем ошибки загрузки - используем пустые данные
            }
        }

        public List<Transaction> GetTransactions()
        {
            if (_data?.Transactions == null)
                return new List<Transaction>();
            return _data.Transactions;
        }

        public List<Category> GetCategories()
        {
            if (_data?.Categories == null)
                return new List<Category>();
            return _data.Categories;
        }

        public List<User> GetUsers()
        {
            if (_data?.Users == null)
                return new List<User>();
            return _data.Users;
        }

        public User? Authenticate(string username, string password)
        {
            if (_data?.Users == null)
                return null;

            return _data.Users.FirstOrDefault(u =>
                string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password);
        }

        public void AddTransaction(Transaction transaction)
        {
            if (_data?.Transactions == null)
                return;
                
            if (transaction.Id == 0)
            {
                transaction.Id = _data.Transactions.Count > 0 
                    ? _data.Transactions.Max(t => t.Id) + 1 
                    : 1;
            }
            _data.Transactions.Add(transaction);
            SaveData();
        }

        public void UpdateTransaction(Transaction transaction)
        {
            var existing = _data.Transactions.FirstOrDefault(t => t.Id == transaction.Id);
            if (existing != null)
            {
                var index = _data.Transactions.IndexOf(existing);
                _data.Transactions[index] = transaction;
                SaveData();
            }
        }

        public void DeleteTransaction(int id)
        {
            var transaction = _data.Transactions.FirstOrDefault(t => t.Id == id);
            if (transaction != null)
            {
                _data.Transactions.Remove(transaction);
                SaveData();
            }
        }

        public void AddCategory(Category category)
        {
            if (_data?.Categories == null)
                return;
                
            if (!_data.Categories.Any(c => c.Name == category.Name && c.Type == category.Type))
            {
                _data.Categories.Add(category);
                SaveData();
            }
        }

        public void DeleteCategory(string name, TransactionType type)
        {
            var category = _data.Categories.FirstOrDefault(c => c.Name == name && c.Type == type);
            if (category != null)
            {
                _data.Categories.Remove(category);
                SaveData();
            }
        }

        public List<BudgetLimit> GetBudgetLimits()
        {
            if (_data?.BudgetLimits == null)
                return new List<BudgetLimit>();
            return _data.BudgetLimits;
        }

        public void AddBudgetLimit(BudgetLimit limit)
        {
            if (_data?.BudgetLimits == null)
                return;
                
            var existing = _data.BudgetLimits.FirstOrDefault(
                l => l.CategoryName == limit.CategoryName && l.Type == limit.Type && l.Period == limit.Period);
            if (existing != null)
            {
                existing.Limit = limit.Limit;
            }
            else
            {
                _data.BudgetLimits.Add(limit);
            }
            SaveData();
        }

        public void DeleteBudgetLimit(string categoryName, TransactionType type, string period)
        {
            if (_data?.BudgetLimits == null)
                return;
            
            var limit = _data.BudgetLimits.FirstOrDefault(
                l => l.CategoryName == categoryName && l.Type == type && l.Period == period);
            if (limit != null)
            {
                _data.BudgetLimits.Remove(limit);
                SaveData();
            }
        }

        public bool AddUser(User user)
        {
            if (_data == null)
            {
                _data = new BudgetData();
            }

            if (_data.Users == null)
            {
                _data.Users = new List<User>();
            }

            if (_data.Users.Any(u => string.Equals(u.Username, user.Username, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            user.Id = _data.Users.Count > 0 ? _data.Users.Max(u => u.Id) + 1 : 1;
            _data.Users.Add(user);
            SaveData();
            return true;
        }

        private void LoadData()
        {
            try
            {
                if (File.Exists(_dataPath))
                {
                    try
                {
                    var json = File.ReadAllText(_dataPath);
                        var loadedData = JsonConvert.DeserializeObject<BudgetData>(json);
                        
                        if (loadedData != null)
                        {
                            _data = loadedData;
                            
                            // Инициализация для старых файлов данных
                            if (_data.Transactions == null)
                                _data.Transactions = new List<Transaction>();
                            if (_data.Categories == null)
                                _data.Categories = new List<Category>();
                            if (_data.BudgetLimits == null)
                                _data.BudgetLimits = new List<BudgetLimit>();
                            if (_data.Users == null)
                                _data.Users = new List<User>();
                            if (_data.Users.Count == 0)
                            {
                                InitializeDefaultUsers();
                                SaveData(showError: false);
                }
                            
                            // Если категорий нет, инициализируем по умолчанию
                            if (_data.Categories.Count == 0)
                            {
                                InitializeDefaultCategories();
                                SaveData(showError: false);
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        // Файл поврежден - используем пустые данные
                    _data = new BudgetData();
                    InitializeDefaultCategories();
                }
            }
                else
                {
                    // Файла нет - инициализируем по умолчанию
                    InitializeDefaultCategories();
                    InitializeDefaultUsers();
                }
            }
            catch (Exception)
            {
                // Любая другая ошибка - используем пустые данные
                _data = new BudgetData();
                try
                {
                    InitializeDefaultCategories();
                    InitializeDefaultUsers();
                }
            catch
            {
                    // Игнорируем ошибки инициализации
                }
            }
        }

        private void SaveData()
        {
            SaveData(showError: true);
        }

        private void SaveData(bool showError)
        {
            try
            {
                if (_data == null)
                    return;
                    
                var directory = Path.GetDirectoryName(_dataPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonConvert.SerializeObject(_data, Formatting.Indented);
                File.WriteAllText(_dataPath, json);
            }
            catch (Exception ex)
            {
                // Не показываем MessageBox во время инициализации (при загрузке XAML)
                if (showError)
            {
                System.Windows.MessageBox.Show($"Ошибка сохранения данных: {ex.Message}", "Ошибка", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private void InitializeDefaultCategories()
        {
            if (_data?.Categories == null)
                return;
                
            _data.Categories.AddRange(new[]
            {
                new Category { Name = "Зарплата", Type = TransactionType.Income, Color = "#4ECDC4" },
                new Category { Name = "Подарки", Type = TransactionType.Income, Color = "#95E1D3" },
                new Category { Name = "Другое", Type = TransactionType.Income, Color = "#F38181" },
                new Category { Name = "Продукты", Type = TransactionType.Expense, Color = "#FF6B6B" },
                new Category { Name = "Транспорт", Type = TransactionType.Expense, Color = "#4ECDC4" },
                new Category { Name = "Развлечения", Type = TransactionType.Expense, Color = "#95E1D3" },
                new Category { Name = "Здоровье", Type = TransactionType.Expense, Color = "#F38181" },
                new Category { Name = "Одежда", Type = TransactionType.Expense, Color = "#AA96DA" },
                new Category { Name = "Коммунальные", Type = TransactionType.Expense, Color = "#FCBAD3" },
                new Category { Name = "Другое", Type = TransactionType.Expense, Color = "#C7CEEA" }
            });
            
            // Не сохраняем при инициализации, чтобы избежать проблем с UI
            // SaveData будет вызван позже, когда пользователь что-то изменит
        }

        private void InitializeDefaultUsers()
        {
            if (_data == null)
            {
                _data = new BudgetData();
            }

            if (_data.Users == null)
            {
                _data.Users = new List<User>();
            }

            if (_data.Users.Count > 0)
                return;

            _data.Users.AddRange(new[]
            {
                new User { Id = 1, Username = "admin", Password = "admin123", Role = UserRole.Admin },
                new User { Id = 2, Username = "user", Password = "user123", Role = UserRole.User }
            });
        }
    }

    public class BudgetData
    {
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<BudgetLimit> BudgetLimits { get; set; } = new List<BudgetLimit>();
        public List<User> Users { get; set; } = new List<User>();
    }
}

