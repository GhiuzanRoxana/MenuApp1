using MenuApp.Models;
using MenuApp.Services;
using MenuApp.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace MenuApp.ViewModels
{
    public class EmployeeOrdersViewModel : ViewModelBase
    {
        private readonly DatabaseService _databaseService;
        private ObservableCollection<Order> _allOrders = new ObservableCollection<Order>();
        private ObservableCollection<Order> _activeOrders = new ObservableCollection<Order>();
        private bool _isLoading;
        private string _statusMessage = "";
        private Order _selectedOrder;
        private DateTime _startDate = DateTime.Today.AddDays(-180); 
        private DateTime _endDate = DateTime.Today;

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
                OnPropertyChanged(nameof(CanChangeOrderStatus));
                OnPropertyChanged(nameof(CanCancelOrder));
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged(nameof(EndDate));
            }
        }

        public bool CanChangeOrderStatus => SelectedOrder != null &&
                                          (SelectedOrder.Status == "inregistrata" ||
                                           SelectedOrder.Status == "se pregateste" ||
                                           SelectedOrder.Status == "a plecat la client");

        public bool CanCancelOrder => SelectedOrder != null &&
                                     (SelectedOrder.Status == "inregistrata" ||
                                      SelectedOrder.Status == "se pregateste");

        public ICommand ChangeOrderStatusCommand { get; }
        public ICommand CancelOrderCommand { get; }
        public ICommand RefreshOrdersCommand { get; }
        public ICommand FilterOrdersCommand { get; }

        public EmployeeOrdersViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;

            ChangeOrderStatusCommand = new RelayCommand(ChangeOrderStatus, () => CanChangeOrderStatus);
            CancelOrderCommand = new RelayCommand(CancelOrder, () => CanCancelOrder);
            RefreshOrdersCommand = new RelayCommand(LoadOrders);
            FilterOrdersCommand = new RelayCommand(LoadOrders);

            LoadOrders();
        }

        private async void LoadOrders()
        {
            IsLoading = true;
            StatusMessage = "Se încarcă comenzile...";

            try
            {
                System.Diagnostics.Debug.WriteLine($"Începe încărcarea comenzilor pentru perioada: {StartDate.ToShortDateString()} - {EndDate.ToShortDateString()}");

                var orders = await _databaseService.GetOrdersInDateRangeAsync(StartDate, EndDate);
                System.Diagnostics.Debug.WriteLine($"Comenzi returnate de la baza de date: {orders.Count}");

                if (orders.Count == 0)
                {
                    StatusMessage = "Nu s-au găsit comenzi în intervalul specificat.";
                    AllOrders.Clear();
                    ActiveOrders.Clear();
                    return;
                }

                var sortedOrders = orders.OrderByDescending(o => o.OrderDate).ToList();
                AllOrders = new ObservableCollection<Order>(sortedOrders);

                var activeOrders = sortedOrders.Where(o =>
                    o.Status != "livrata" && o.Status != "anulata").ToList();
                ActiveOrders = new ObservableCollection<Order>(activeOrders);

                StatusMessage = $"{AllOrders.Count} comenzi în total și {ActiveOrders.Count} comenzi active.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la încărcarea comenzilor: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Excepție la încărcarea comenzilor: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void ChangeOrderStatus()
        {
            if (SelectedOrder == null)
                return;

            var currentStatus = SelectedOrder.Status;
            string newStatus = "";

            switch (currentStatus)
            {
                case "inregistrata":
                    newStatus = "se pregateste";
                    break;
                case "se pregateste":
                    newStatus = "a plecat la client";
                    break;
                case "a plecat la client":
                    newStatus = "livrata";
                    break;
                default:
                    return;
            }

            IsLoading = true;

            try
            {
                bool success = await _databaseService.UpdateOrderStatusAsync(SelectedOrder.OrderId, newStatus);

                if (success)
                {
                    StatusMessage = $"Starea comenzii a fost actualizată cu succes la '{newStatus}'.";
                    LoadOrders();
                }
                else
                {
                    StatusMessage = "Nu s-a putut actualiza starea comenzii.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la actualizarea stării comenzii: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Excepție la actualizarea statusului: {ex.Message}");
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
                    StatusMessage = "Comanda a fost anulată cu succes.";
                    LoadOrders();
                }
                else
                {
                    StatusMessage = "Nu s-a putut anula comanda.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la anularea comenzii: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Excepție la anularea comenzii: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}