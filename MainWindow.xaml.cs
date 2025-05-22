using MenuApp.Services;
using MenuApp.ViewModels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MenuApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            try
            {
                string connectionString = "Host=localhost;Port=5432;Database=steaua_de_mare;Username=postgres;Password=1q2w3e";

                var databaseService = new DatabaseService(connectionString);

                var mainViewModel = new MainViewModel(databaseService);

                DataContext = mainViewModel;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la inițializarea aplicației: {ex.Message}", "Eroare",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}