using BudgetApp.Models;

namespace BudgetApp.Services
{
    public class AuthService
    {
        private readonly DataService _dataService;

        public AuthService(DataService dataService)
        {
            _dataService = dataService;
        }

        public User? Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            _dataService.EnsureDataLoaded();
            return _dataService.Authenticate(username.Trim(), password);
        }

        public bool Register(string username, string password, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            {
                errorMessage = "Имя пользователя должно содержать не менее 3 символов.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 4)
            {
                errorMessage = "Пароль должен содержать не менее 4 символов.";
                return false;
            }

            _dataService.EnsureDataLoaded();

            var success = _dataService.AddUser(new User
            {
                Username = username.Trim(),
                Password = password,
                Role = UserRole.User
            });

            if (!success)
            {
                errorMessage = "Пользователь с таким именем уже существует.";
            }

            return success;
        }
    }
}

