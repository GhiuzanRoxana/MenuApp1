using MenuApp.Services;
using MenuApp.ViewModels;
using System;
using System.Windows;

namespace MenuApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            try
            {
                string exePath = AppDomain.CurrentDomain.BaseDirectory;
                string imagesFolder = System.IO.Path.Combine(exePath, "Images");
                if (!System.IO.Directory.Exists(imagesFolder))
                {
                    System.IO.Directory.CreateDirectory(imagesFolder);
                }

                string connectionString = "Host=localhost;Database=steaua_de_mare;Username=postgres;Password=1q2w3e";
                var databaseService = new DatabaseService(connectionString);

                var cartService = CartService.Instance;
                var userService = UserService.Instance;

                var mainViewModel = new MainViewModel(databaseService);
                MainWindow = new MainWindow
                {
                    DataContext = mainViewModel
                };
                MainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"A apărut o eroare la pornirea aplicației: {ex.Message}",
                                "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}