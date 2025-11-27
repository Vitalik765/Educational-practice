using System.Windows;
using BudgetApp.Models;
using BudgetApp.Services;

namespace BudgetApp.Windows
{
    public partial class LoginWindow : Window
    {
        private readonly AuthService _authService;
        public User? AuthenticatedUser { get; private set; }

        public LoginWindow(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            UsernameTextBox.Focus();
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = string.IsNullOrWhiteSpace(message)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text;
            var password = PasswordBox.Password;

            var user = _authService.Login(username, password);
            if (user == null)
            {
                ShowError("Неверное имя пользователя или пароль.");
                return;
            }

            AuthenticatedUser = user;
            DialogResult = true;
            Close();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            var registrationWindow = new RegistrationWindow(_authService);
            if (registrationWindow.ShowDialog() == true)
            {
                UsernameTextBox.Text = registrationWindow.CreatedUsername;
                PasswordBox.Password = string.Empty;
                ShowError("Успешная регистрация. Введите пароль и войдите.");
                UsernameTextBox.Focus();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

