using MenuApp.Models;
using MenuApp.Services;
using MenuApp.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MenuApp.ViewModels
{
    public class ClientOrdersViewModel : ViewModelBase
    {
        private readonly DatabaseService _databaseService;
        private readonly int _userId;
        private ObservableCollection<Order> _allOrders = new ObservableCollection<Order>();
        private ObservableCollection<Order> _activeOrders = new ObservableCollection<Order>();
        private bool _isLoading;
        private string _statusMessage;
        private Order _selectedOrder;

        public ObservableCollection<Order> AllOrders
        {
            get => _allOrders;
            set
            {
                _allOrders = value;
                OnPropertyChanged(nameof(AllOrders));
            }
        }

        public ObservableCollection<Order> ActiveOrders
        {
            get => _activeOrders;
            set
            {
                _activeOrders = value;
                OnPropertyChanged(nameof(ActiveOrders));
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

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged(nameof(SelectedOrder));
                OnPropertyChanged(nameof(CanCancelOrder));
            }
        }

        public bool CanCancelOrder => SelectedOrder != null &&
                                     (SelectedOrder.Status == "inregistrata" ||
                                      SelectedOrder.Status == "se pregateste");

        public ICommand CancelOrderCommand { get; }
        public ICommand RefreshOrdersCommand { get; }

        public ClientOrdersViewModel(DatabaseService databaseService, int userId)
        {
            _databaseService = databaseService;
            _userId = userId;

            CancelOrderCommand = new RelayCommand(CancelOrder, () => CanCancelOrder);
            RefreshOrdersCommand = new RelayCommand(LoadOrders);

            LoadOrders();
        }

        private async void LoadOrders()
        {
            IsLoading = true;
            StatusMessage = "Se încarcă comenzile...";

            try
            {
                var orders = await _databaseService.GetUserOrdersAsync(_userId);

                var sortedOrders = orders.OrderByDescending(o => o.OrderDate).ToList();

                AllOrders = new ObservableCollection<Order>(sortedOrders);

                var activeOrders = sortedOrders.Where(o =>
                    o.Status != "livrata" && o.Status != "anulata").ToList();

                ActiveOrders = new ObservableCollection<Order>(activeOrders);

                if (AllOrders.Count == 0)
                {
                    StatusMessage = "Nu aveți comenzi.";
                }
                else
                {
                    StatusMessage = $"Aveți {AllOrders.Count} comenzi în total și {ActiveOrders.Count} comenzi active.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la încărcarea comenzilor: {ex.Message}";
                MessageBox.Show($"Eroare la încărcarea comenzilor: {ex.Message}", "Eroare",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void CancelOrder()
        {
            if (SelectedOrder == null)
                return;

            IsLoading = true;

            try
            {
                bool success = await _databaseService.CancelOrderAsync(SelectedOrder.OrderId);

                if (success)
                {
                    MessageBox.Show("Comanda a fost anulată cu succes.", "Succes",
                                   MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadOrders();
                }
                else
                {
                    MessageBox.Show("Nu s-a putut anula comanda.", "Eroare",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la anularea comenzii: {ex.Message}", "Eroare",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}