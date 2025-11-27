using System;
using System.Windows;
using BudgetApp.Models;
using BudgetApp.Services;
using BudgetApp.Windows;

namespace BudgetApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Обработка необработанных исключений
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            // Предотвращаем автоматическое завершение приложения при закрытии окна входа
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var dataService = new DataService();
            dataService.EnsureDataLoaded();
            var authService = new AuthService(dataService);
            var loginWindow = new LoginWindow(authService);

            if (loginWindow.ShowDialog() == true && loginWindow.AuthenticatedUser != null)
            {
                var mainWindow = new MainWindow(dataService, loginWindow.AuthenticatedUser);
                Current.MainWindow = mainWindow;
                ShutdownMode = ShutdownMode.OnMainWindowClose;
                mainWindow.Show();
            }
            else
            {
                Shutdown();
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                $"Произошла ошибка:\n\n{e.Exception.Message}\n\n{e.Exception.StackTrace}",
                "Ошибка приложения",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            MessageBox.Show(
                $"Критическая ошибка:\n\n{exception?.Message ?? "Неизвестная ошибка"}\n\n{exception?.StackTrace}",
                "Критическая ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}

