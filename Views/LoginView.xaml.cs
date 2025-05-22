using MenuApp.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MenuApp.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.Password = PasswordBox.Password;
                viewModel.LoginCommand.Execute(null);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.NewUser.Password = RegisterPasswordBox.Password;
                viewModel.RegisterCommand.Execute(null);
            }
        }

        private void RegisterText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.ShowRegisterForm = true;
            }
        }

        private void LoginText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.ShowRegisterForm = false;
            }
        }
    }
}