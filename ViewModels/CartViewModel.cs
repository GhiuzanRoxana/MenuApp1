using MenuApp.Models;
using MenuApp.Services;
using MenuApp.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace MenuApp.ViewModels
{
    public class CartViewModel : ViewModelBase
    {
        private readonly CartService _cartService;
        private readonly DatabaseService _databaseService;
        private readonly UserService _userService;
        private bool _isCartEmpty = true;
        private decimal _totalPrice = 0M;

        public ObservableCollection<CartItem> CartItems => _cartService.Items;

        public decimal TotalPrice
        {
            get => _totalPrice;
            set
            {
                _totalPrice = value;
                OnPropertyChanged(nameof(TotalPrice));
            }
        }

        public int ItemCount
        {
            get => _cartService.ItemCount;
        }

        public bool IsCartEmpty
        {
            get => _isCartEmpty;
            set
            {
                _isCartEmpty = value;
                OnPropertyChanged(nameof(IsCartEmpty));
            }
        }

        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand CheckoutCommand { get; }

        public CartViewModel(CartService cartService, DatabaseService databaseService, UserService userService)
        {
            Debug.WriteLine("Inițializare CartViewModel...");

            _cartService = cartService;
            _databaseService = databaseService;
            _userService = userService;

            Debug.WriteLine($"Stare inițială: ItemCount={_cartService.ItemCount}, TotalPrice={_cartService.TotalPrice}");
            UpdateCartStatus();

            foreach (var item in _cartService.Items)
            {
                Debug.WriteLine($"Produs: {item.Name}, Cantitate: {item.Quantity}, Preț: {item.Price:N2} lei");
            }

            _cartService.CartChanged += (s, e) =>
            {
                Debug.WriteLine("Event CartChanged declanșat");
                UpdateCartStatus();

                OnPropertyChanged(nameof(CartItems));
                OnPropertyChanged(nameof(ItemCount));
                OnPropertyChanged(nameof(TotalPrice));
                OnPropertyChanged(nameof(IsCartEmpty));
            };

            IncreaseQuantityCommand = new RelayCommand<CartItem>(IncreaseQuantity);
            DecreaseQuantityCommand = new RelayCommand<CartItem>(DecreaseQuantity);
            RemoveItemCommand = new RelayCommand<CartItem>(RemoveItem);
            ClearCartCommand = new RelayCommand(ClearCart);
            CheckoutCommand = new RelayCommand(Checkout, CanCheckout);

            Debug.WriteLine("CartViewModel inițializat cu succes");
        }

        private void UpdateCartStatus()
        {
            Debug.WriteLine("UpdateCartStatus apelat...");

            TotalPrice = _cartService.TotalPrice;

            IsCartEmpty = _cartService.ItemCount <= 0;

            Debug.WriteLine($"Stare actualizată: ItemCount={ItemCount}, TotalPrice={TotalPrice:N2}, IsCartEmpty={IsCartEmpty}");

            Debug.WriteLine("Produse în coș:");
            foreach (var item in _cartService.Items)
            {
                Debug.WriteLine($"   - {item.Name}, Cantitate: {item.Quantity}, Preț: {item.Price:N2} lei");
            }
        }

        private void IncreaseQuantity(CartItem item)
        {
            if (item != null)
            {
                Debug.WriteLine($"Creștere cantitate pentru: {item.Name}");
                _cartService.UpdateItemQuantity(item, item.Quantity + 1);
            }
        }

        private void DecreaseQuantity(CartItem item)
        {
            if (item != null && item.Quantity > 1)
            {
                Debug.WriteLine($"Scădere cantitate pentru: {item.Name}");
                _cartService.UpdateItemQuantity(item, item.Quantity - 1);
            }
        }

        private void RemoveItem(CartItem item)
        {
            if (item != null)
            {
                Debug.WriteLine($"Eliminare produs: {item.Name}");
                _cartService.RemoveItem(item);
            }
        }

        private void ClearCart()
        {
            Debug.WriteLine("Golire coș...");
            _cartService.ClearCart();
        }

        private bool CanCheckout()
        {
            return _cartService.ItemCount > 0 && _userService.IsAuthenticated;
        }

        private async void Checkout()
        {
            if (!_userService.IsAuthenticated)
            {
                MessageBox.Show("Trebuie să fiți autentificat pentru a plasa o comandă.", "Autentificare necesară",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_cartService.ItemCount == 0)
            {
                MessageBox.Show("Coșul este gol.", "Comandă", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var order = await _cartService.CreateOrderFromCartAsync(_userService.CurrentUser, _databaseService);

                if (order != null)
                {
                    MessageBox.Show($"Comanda ta cu codul {order.OrderCode} a fost plasată cu succes!",
                                  "Comandă plasată", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Nu s-a putut plasa comanda. Verificați din nou coșul și încercați mai târziu.",
                                  "Eroare la plasarea comenzii", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la plasarea comenzii: {ex.Message}",
                              "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}