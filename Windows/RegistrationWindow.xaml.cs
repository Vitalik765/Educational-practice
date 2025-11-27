using System.Windows;
using BudgetApp.Services;

namespace BudgetApp.Windows
{
    public partial class RegistrationWindow : Window
    {
        private readonly AuthService _authService;
        public string CreatedUsername { get; private set; } = string.Empty;

        public RegistrationWindow(AuthService authService)
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

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text.Trim();
            var password = PasswordBox.Password;
            var confirm = ConfirmPasswordBox.Password;

            if (password != confirm)
            {
                ShowError("Пароли не совпадают.");
                return;
            }

            if (_authService.Register(username, password, out var error))
            {
                CreatedUsername = username;
                DialogResult = true;
                Close();
            }
            else
            {
                ShowError(error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

