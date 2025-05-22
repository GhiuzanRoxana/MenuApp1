using MenuApp.Models;
using MenuApp.Services;
using MenuApp.Utilities;
using MenuApp.Views;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MenuApp.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly DatabaseService _databaseService;
        private string _email;
        private string _password;
        private bool _isLoading;
        private string _errorMessage;
        private bool _showRegisterForm;
        private User _newUser = new User();

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public bool ShowRegisterForm
        {
            get => _showRegisterForm;
            set
            {
                _showRegisterForm = value;
                OnPropertyChanged(nameof(ShowRegisterForm));
                OnPropertyChanged(nameof(ShowLoginForm));
            }
        }

        public bool ShowLoginForm => !ShowRegisterForm;

        public User NewUser
        {
            get => _newUser;
            set
            {
                _newUser = value;
                OnPropertyChanged(nameof(NewUser));
            }
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand ShowRegisterFormCommand { get; }
        public ICommand ShowLoginFormCommand { get; }

        public LoginViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            LoginCommand = new RelayCommand(ExecuteLogin);
            RegisterCommand = new RelayCommand(ExecuteRegister);
            ShowRegisterFormCommand = new RelayCommand(() => ShowRegisterForm = true);
            ShowLoginFormCommand = new RelayCommand(() => ShowRegisterForm = false);
        }

        private async void ExecuteLogin()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Introduceți email-ul și parola.";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var user = await _databaseService.AuthenticateUserAsync(Email, Password);

                if (user != null)
                {
                    UserService.Instance.SetCurrentUser(user);

                    MessageBox.Show($"Bine ai venit, {user.FirstName}!", "Autentificare reușită",
                                    MessageBoxButton.OK, MessageBoxImage.Information);

                    Email = string.Empty;
                    Password = string.Empty;
                }
                else
                {
                    ErrorMessage = "Email sau parolă incorectă.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la autentificare: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void ExecuteRegister()
        {
            if (string.IsNullOrWhiteSpace(NewUser.FirstName) ||
                string.IsNullOrWhiteSpace(NewUser.LastName) ||
                string.IsNullOrWhiteSpace(NewUser.Email) ||
                string.IsNullOrWhiteSpace(NewUser.PhoneNumber) ||
                string.IsNullOrWhiteSpace(NewUser.Password))
            {
                ErrorMessage = "Completați toate câmpurile obligatorii.";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                bool success = await _databaseService.RegisterUserAsync(NewUser);

                if (success)
                {
                    MessageBox.Show("Cont creat cu succes! Acum vă puteți autentifica.", "Înregistrare reușită",
                                    MessageBoxButton.OK, MessageBoxImage.Information);

                    NewUser = new User();
                    ShowRegisterForm = false;
                }
                else
                {
                    ErrorMessage = "Înregistrare eșuată. Email-ul poate fi deja utilizat.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la înregistrare: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}