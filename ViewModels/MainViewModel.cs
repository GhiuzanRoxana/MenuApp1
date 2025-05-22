// 4. MainViewModel.cs - complet modificat
using MenuApp.Models;
using MenuApp.Services;
using MenuApp.Utilities;
using MenuApp.Views;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MenuApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly DatabaseService _databaseService;
        private readonly CartService _cartService;
        private readonly UserService _userService;

        private UserControl _currentView;
        private bool _isMenuVisible = true;
        private bool _isSearchVisible = true;
        private bool _isLoginVisible = true;
        private bool _isOrdersVisible = false;
        private bool _isAdminPanelVisible = false;
        private bool _isCartVisible = false;
        public bool IsContactVisible => true;

        public UserControl CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        public bool IsMenuVisible
        {
            get => _isMenuVisible;
            set
            {
                _isMenuVisible = value;
                OnPropertyChanged(nameof(IsMenuVisible));
            }
        }

        public bool IsSearchVisible
        {
            get => _isSearchVisible;
            set
            {
                _isSearchVisible = value;
                OnPropertyChanged(nameof(IsSearchVisible));
            }
        }

        public bool IsLoginVisible
        {
            get => _isLoginVisible;
            set
            {
                _isLoginVisible = value;
                OnPropertyChanged(nameof(IsLoginVisible));
            }
        }

        public bool IsOrdersVisible
        {
            get => _isOrdersVisible;
            set
            {
                _isOrdersVisible = value;
                OnPropertyChanged(nameof(IsOrdersVisible));
            }
        }

        public bool IsAdminPanelVisible
        {
            get => _isAdminPanelVisible;
            set
            {
                _isAdminPanelVisible = value;
                OnPropertyChanged(nameof(IsAdminPanelVisible));
            }
        }

        public bool IsCartVisible
        {
            get => _isCartVisible;
            set
            {
                if (_isCartVisible != value)
                {
                    _isCartVisible = value;
                    System.Diagnostics.Debug.WriteLine($"IsCartVisible changed to: {value}");
                    OnPropertyChanged(nameof(IsCartVisible));
                }
            }
        }

        private void NavigateToContact()
        {
            CurrentView = new ContactView();
        }

        public string UserStatusText => UserService.Instance.IsAuthenticated
            ? $"Bun venit, {UserService.Instance.CurrentUser?.FirstName ?? "Utilizator"}! ({(UserService.Instance.IsEmployee ? "Angajat" : "Client")})"
            : "Vizitator";

        public bool IsAuthenticated => UserService.Instance.IsAuthenticated;
        public bool IsEmployee => UserService.Instance.IsEmployee;
        public bool IsClient => UserService.Instance.IsClient;

        public ICommand NavigateToMenuCommand { get; }
        public ICommand NavigateToSearchCommand { get; }
        public ICommand NavigateToLoginCommand { get; }
        public ICommand NavigateToOrdersCommand { get; }
        public ICommand NavigateToAdminPanelCommand { get; }
        public ICommand NavigateToCartCommand { get; }
        public ICommand LogoutCommand { get; }

        public ICommand NavigateToContactCommand { get; }

        public MainViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _cartService = CartService.Instance;
            _userService = UserService.Instance;

            NavigateToMenuCommand = new RelayCommand(NavigateToMenu);
            NavigateToSearchCommand = new RelayCommand(NavigateToSearch);
            NavigateToLoginCommand = new RelayCommand(NavigateToLogin);
            NavigateToOrdersCommand = new RelayCommand(NavigateToOrders);
            NavigateToAdminPanelCommand = new RelayCommand(NavigateToAdminPanel);
            LogoutCommand = new RelayCommand(Logout);
            NavigateToCartCommand = new RelayCommand(NavigateToCart);
            NavigateToContactCommand = new RelayCommand(NavigateToContact);

            UserService.Instance.UserChanged += UserService_UserChanged;
            UserService.Instance.AuthenticationStatusChanged += UserService_AuthenticationStatusChanged;

            UpdateButtonVisibility();

            NavigateToMenu();
        }

        private void UserService_UserChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(UserStatusText));
            OnPropertyChanged(nameof(IsAuthenticated));
            OnPropertyChanged(nameof(IsEmployee));
            OnPropertyChanged(nameof(IsClient));
        }

        private void UserService_AuthenticationStatusChanged(object sender, EventArgs e)
        {
            UpdateButtonVisibility();
        }

        private void UpdateButtonVisibility()
        {
            System.Diagnostics.Debug.WriteLine("UpdateButtonVisibility called...");

            var userType = UserService.Instance.CurrentUserType;
            System.Diagnostics.Debug.WriteLine($"Current UserType: {userType}");

            switch (userType)
            {
                case UserType.Guest:
                    IsMenuVisible = true;
                    IsSearchVisible = true;
                    IsLoginVisible = true;
                    IsOrdersVisible = false;
                    IsAdminPanelVisible = false;
                    IsCartVisible = false; 
                    break;

                case UserType.Client:
                    IsMenuVisible = true;
                    IsSearchVisible = true;
                    IsLoginVisible = false;
                    IsOrdersVisible = true;
                    IsAdminPanelVisible = false;
                    IsCartVisible = true; 
                    break;

                case UserType.Employee:
                    IsMenuVisible = true;
                    IsSearchVisible = true;
                    IsLoginVisible = false;
                    IsOrdersVisible = true;
                    IsAdminPanelVisible = true;
                    IsCartVisible = false;
                    break;
            }

            System.Diagnostics.Debug.WriteLine($"After update - IsCartVisible: {IsCartVisible}");
        }

        private void NavigateToMenu()
        {
            try
            {
                var menuViewModel = new MenuViewModel(_databaseService);
                var menuView = new MenuView
                {
                    DataContext = menuViewModel
                };
                CurrentView = menuView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea meniului: {ex.Message}",
                               "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateToSearch()
        {
            try
            {
                var searchViewModel = new SearchViewModel(_databaseService);
                var searchView = new SearchView
                {
                    DataContext = searchViewModel
                };
                CurrentView = searchView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea paginii de căutare: {ex.Message}",
                               "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateToLogin()
        {
            try
            {
                var loginViewModel = new LoginViewModel(_databaseService);
                var loginView = new LoginView
                {
                    DataContext = loginViewModel
                };
                CurrentView = loginView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea paginii de autentificare: {ex.Message}",
                               "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateToOrders()
        {
            try
            {
                if (!_userService.IsAuthenticated)
                {
                    MessageBox.Show("Trebuie să fiți autentificat pentru a vizualiza comenzile.",
                                    "Autentificare necesară", MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigateToLogin();
                    return;
                }

                if (_userService.IsEmployee)
                {
                    var ordersViewModel = new EmployeeOrdersViewModel(_databaseService);
                    var ordersView = new EmployeeOrdersView
                    {
                        DataContext = ordersViewModel
                    };
                    CurrentView = ordersView;
                }
                else if (_userService.IsClient)
                {
                    var ordersViewModel = new ClientOrdersViewModel(_databaseService, _userService.CurrentUser.UserId);
                    var ordersView = new ClientOrdersView
                    {
                        DataContext = ordersViewModel
                    };
                    CurrentView = ordersView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea comenzilor: {ex.Message}",
                               "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateToCart()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("NavigateToCart called...");

                if (!_userService.IsAuthenticated)
                {
                    MessageBox.Show("Trebuie să fiți autentificat pentru a accesa coșul.",
                                   "Autentificare necesară", MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigateToLogin();
                    return;
                }

                System.Diagnostics.Debug.WriteLine("Creating CartViewModel...");
                var cartViewModel = new CartViewModel(_cartService, _databaseService, _userService);

                System.Diagnostics.Debug.WriteLine("Creating CartView...");
                var cartView = new CartView
                {
                    DataContext = cartViewModel
                };

                System.Diagnostics.Debug.WriteLine("Setting CurrentView...");
                CurrentView = cartView;

                System.Diagnostics.Debug.WriteLine("NavigateToCart completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Eroare în NavigateToCart: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Eroare la accesarea coșului: {ex.Message}",
                               "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateToAdminPanel()
        {
            try
            {
                if (!_userService.IsEmployee)
                {
                    MessageBox.Show("Acces interzis. Doar angajații pot accesa panoul de administrare.",
                                   "Acces interzis", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var adminViewModel = new AdminPanelViewModel(_databaseService);
                var adminView = new AdminPanelView
                {
                    DataContext = adminViewModel
                };
                CurrentView = adminView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la accesarea panoului de administrare: {ex.Message}",
                               "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Logout()
        {
            try
            {
                _userService.Logout();
                _cartService.ClearCart();

                UpdateButtonVisibility();

                NavigateToMenu();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la deconectare: {ex.Message}",
                               "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}