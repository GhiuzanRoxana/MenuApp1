using MenuApp.Models;
using MenuApp.Services;
using MenuApp.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace MenuApp.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        private readonly DatabaseService _databaseService;
        private readonly CartService _cartService;
        private readonly UserService _userService;
        private bool _isLoading;
        private ObservableCollection<CategoryWithDishes> _categories = new ObservableCollection<CategoryWithDishes>();

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public ObservableCollection<CategoryWithDishes> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged(nameof(Categories));
            }
        }

        public ICommand AddToCartCommand { get; private set; }

        public MenuViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _cartService = CartService.Instance;
            _userService = UserService.Instance;

            AddToCartCommand = new RelayCommand<Dish>(AddToCart);

            LoadMenu();
        }

        private async void LoadMenu()
        {
            IsLoading = true;

            try
            {
                var categories = await _databaseService.GetCategoriesAsync();
                var tempCategories = new ObservableCollection<CategoryWithDishes>();

                foreach (var category in categories)
                {
                    var dishes = await _databaseService.GetDishesByCategoryAsync(category.CategoryId);
                    if (dishes.Count > 0)
                    {
                        var categoryWithDishes = new CategoryWithDishes
                        {
                            CategoryId = category.CategoryId,
                            Name = category.Name,
                            Description = category.Description
                        };

                        foreach (var dish in dishes)
                        {
                            dish.IsAvailable = dish.TotalQuantity >= dish.PortionQuantity;
                            categoryWithDishes.Dishes.Add(dish);
                        }

                        tempCategories.Add(categoryWithDishes);
                    }
                }

                Categories = tempCategories;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea meniului: {ex.Message}",
                               "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddToCart(Dish dish)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"AddToCart apelat cu: ID={dish?.DishId ?? -1}, Nume={dish?.Name ?? "null"}, Disponibil={dish?.IsAvailable ?? false}");

                if (!_userService.IsAuthenticated)
                {
                    MessageBox.Show("Trebuie să fiți autentificat pentru a adăuga produse în coș.",
                                   "Autentificare necesară", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (dish == null)
                {
                    System.Diagnostics.Debug.WriteLine("Dish este null!");
                    return;
                }

                if (dish.TotalQuantity < dish.PortionQuantity)
                {
                    MessageBox.Show($"{dish.Name} nu este disponibil în stoc.",
                                   "Produs indisponibil", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (string.IsNullOrEmpty(dish.Name))
                {
                    dish.Name = $"Produs #{dish.DishId}";
                    System.Diagnostics.Debug.WriteLine($"Nume produs null detectat! Setat nume implicit: {dish.Name}");
                }

                _cartService.AddItem(dish);

                MessageBox.Show($"{dish.Name} a fost adăugat în coș.",
                               "Succes", MessageBoxButton.OK, MessageBoxImage.Information);

                System.Diagnostics.Debug.WriteLine($"Produs adăugat cu succes. Coș total: {_cartService.ItemCount} produse, Total: {_cartService.TotalPrice:N2} lei");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Eroare la adăugarea în coș: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Eroare la adăugarea în coș: {ex.Message}",
                               "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class CategoryWithDishes : Category
    {
        public ObservableCollection<Dish> Dishes { get; set; } = new ObservableCollection<Dish>();
    }
}