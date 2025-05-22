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
    public class AdminPanelViewModel : ViewModelBase
    {
        private readonly DatabaseService _databaseService;
        private ObservableCollection<Category> _categories = new ObservableCollection<Category>();
        private ObservableCollection<Dish> _dishes = new ObservableCollection<Dish>();
        private ObservableCollection<Menu> _menus = new ObservableCollection<Menu>();
        private ObservableCollection<Allergen> _allergens = new ObservableCollection<Allergen>();
        private ObservableCollection<Dish> _lowStockDishes = new ObservableCollection<Dish>();
        private ObservableCollection<Order> _allOrders = new ObservableCollection<Order>();
        private ObservableCollection<Order> _activeOrders = new ObservableCollection<Order>();

        private Category _selectedCategory;
        private Dish _selectedDish;
        private Menu _selectedMenu;
        private Allergen _selectedAllergen;
        private Order _selectedOrder;

        private bool _isLoading;
        private string _statusMessage;
        private int _selectedTabIndex;

        private Category _editCategory = new Category();
        private Dish _editDish = new Dish();
        private Menu _editMenu = new Menu();
        private Allergen _editAllergen = new Allergen();

        private bool _isEditMode;

        private DateTime _startDate = DateTime.Today.AddDays(-30);
        private DateTime _endDate = DateTime.Today;

        #region Properties

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged(nameof(Categories));
            }
        }

        public ObservableCollection<Dish> Dishes
        {
            get => _dishes;
            set
            {
                _dishes = value;
                OnPropertyChanged(nameof(Dishes));
            }
        }

        public ObservableCollection<Menu> Menus
        {
            get => _menus;
            set
            {
                _menus = value;
                OnPropertyChanged(nameof(Menus));
            }
        }

        public ObservableCollection<Allergen> Allergens
        {
            get => _allergens;
            set
            {
                _allergens = value;
                OnPropertyChanged(nameof(Allergens));
            }
        }

        public ObservableCollection<Dish> LowStockDishes
        {
            get => _lowStockDishes;
            set
            {
                _lowStockDishes = value;
                OnPropertyChanged(nameof(LowStockDishes));
            }
        }

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

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));

                if (value != null)
                {
                    EditCategory = new Category
                    {
                        CategoryId = value.CategoryId,
                        Name = value.Name,
                        Description = value.Description
                    };
                }
            }
        }

        public Dish SelectedDish
        {
            get => _selectedDish;
            set
            {
                _selectedDish = value;
                OnPropertyChanged(nameof(SelectedDish));

                if (value != null)
                {
                    EditDish = new Dish
                    {
                        DishId = value.DishId,
                        Name = value.Name,
                        Price = value.Price,
                        PortionQuantity = value.PortionQuantity,
                        TotalQuantity = value.TotalQuantity,
                        CategoryId = value.CategoryId
                    };
                }
            }
        }

        public Menu SelectedMenu
        {
            get => _selectedMenu;
            set
            {
                _selectedMenu = value;
                OnPropertyChanged(nameof(SelectedMenu));

                if (value != null)
                {
                    EditMenu = new Menu
                    {
                        MenuId = value.MenuId,
                        Name = value.Name,
                        CategoryId = value.CategoryId
                    };
                }
            }
        }

        public Allergen SelectedAllergen
        {
            get => _selectedAllergen;
            set
            {
                _selectedAllergen = value;
                OnPropertyChanged(nameof(SelectedAllergen));

                if (value != null)
                {
                    EditAllergen = new Allergen
                    {
                        AllergenId = value.AllergenId,
                        Name = value.Name,
                        Description = value.Description
                    };
                }
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

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                _selectedTabIndex = value;
                OnPropertyChanged(nameof(SelectedTabIndex));

                switch (value)
                {
                    case 0: 
                        LoadCategories();
                        break;
                    case 1: 
                        LoadDishes();
                        break;
                    case 2: 
                        LoadMenus();
                        break;
                    case 3: 
                        LoadAllergens();
                        break;
                    case 4: 
                        LoadLowStockDishes();
                        break;
                    case 5:
                        LoadOrders();
                        break;
                }
            }
        }

        public Category EditCategory
        {
            get => _editCategory;
            set
            {
                _editCategory = value;
                OnPropertyChanged(nameof(EditCategory));
            }
        }

        public Dish EditDish
        {
            get => _editDish;
            set
            {
                _editDish = value;
                OnPropertyChanged(nameof(EditDish));
            }
        }

        public Menu EditMenu
        {
            get => _editMenu;
            set
            {
                _editMenu = value;
                OnPropertyChanged(nameof(EditMenu));
            }
        }

        public Allergen EditAllergen
        {
            get => _editAllergen;
            set
            {
                _editAllergen = value;
                OnPropertyChanged(nameof(EditAllergen));
            }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                _isEditMode = value;
                OnPropertyChanged(nameof(IsEditMode));
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

        #endregion

        #region Commands

        public ICommand SaveCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }
        public ICommand NewCategoryCommand { get; }

        public ICommand SaveDishCommand { get; }
        public ICommand DeleteDishCommand { get; }
        public ICommand NewDishCommand { get; }

        public ICommand SaveMenuCommand { get; }
        public ICommand DeleteMenuCommand { get; }
        public ICommand NewMenuCommand { get; }

        public ICommand SaveAllergenCommand { get; }
        public ICommand DeleteAllergenCommand { get; }
        public ICommand NewAllergenCommand { get; }

        public ICommand FilterOrdersCommand { get; }
        public ICommand ChangeOrderStatusCommand { get; }
        public ICommand CancelOrderCommand { get; }

        public ICommand RefreshDataCommand { get; }



        #endregion
        public AdminPanelViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;

            SaveCategoryCommand = new RelayCommand(SaveCategory);
            DeleteCategoryCommand = new RelayCommand(DeleteCategory, () => SelectedCategory != null);
            NewCategoryCommand = new RelayCommand(NewCategory);

            SaveDishCommand = new RelayCommand(SaveDish);
            DeleteDishCommand = new RelayCommand(DeleteDish, () => SelectedDish != null);
            NewDishCommand = new RelayCommand(NewDish);

            SaveMenuCommand = new RelayCommand(SaveMenu);
            DeleteMenuCommand = new RelayCommand(DeleteMenu, () => SelectedMenu != null);
            NewMenuCommand = new RelayCommand(NewMenu);

            SaveAllergenCommand = new RelayCommand(SaveAllergen);
            DeleteAllergenCommand = new RelayCommand(DeleteAllergen, () => SelectedAllergen != null);
            NewAllergenCommand = new RelayCommand(NewAllergen);

            FilterOrdersCommand = new RelayCommand(LoadOrders);
            ChangeOrderStatusCommand = new RelayCommand(ChangeOrderStatus, () => CanChangeOrderStatus);
            CancelOrderCommand = new RelayCommand(CancelOrder, () => SelectedOrder != null && SelectedOrder.Status != "livrata" && SelectedOrder.Status != "anulata");

            RefreshDataCommand = new RelayCommand(RefreshData);

            BrowseImageCommand = new RelayCommand(BrowseImage);

            SelectedTabIndex = 0;
        }

        #region Data Loading Methods

        private async void LoadCategories()
        {
            IsLoading = true;
            StatusMessage = "Se încarcă categoriile...";

            try
            {
                var categories = await _databaseService.GetCategoriesAsync();
                Categories = new ObservableCollection<Category>(categories);

                if (Categories.Count == 0)
                {
                    StatusMessage = "Nu există categorii.";
                }
                else
                {
                    StatusMessage = $"{Categories.Count} categorii încărcate.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la încărcarea categoriilor: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void LoadDishes()
        {
            IsLoading = true;
            StatusMessage = "Se încarcă preparatele...";

            try
            {
                var dishes = await _databaseService.GetAllDishesAsync();
                Dishes = new ObservableCollection<Dish>(dishes);

                if (Dishes.Count == 0)
                {
                    StatusMessage = "Nu există preparate.";
                }
                else
                {
                    StatusMessage = $"{Dishes.Count} preparate încărcate.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la încărcarea preparatelor: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void LoadMenus()
        {
            IsLoading = true;
            StatusMessage = "Se încarcă meniurile...";

            try
            {
                var menus = await _databaseService.GetAllMenusAsync();
                Menus = new ObservableCollection<Menu>(menus);

                if (Menus.Count == 0)
                {
                    StatusMessage = "Nu există meniuri.";
                }
                else
                {
                    StatusMessage = $"{Menus.Count} meniuri încărcate.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la încărcarea meniurilor: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void LoadAllergens()
        {
            IsLoading = true;
            StatusMessage = "Se încarcă alergenii...";

            try
            {
                var allergens = await _databaseService.GetAllAllergensAsync();
                Allergens = new ObservableCollection<Allergen>(allergens);

                if (Allergens.Count == 0)
                {
                    StatusMessage = "Nu există alergeni.";
                }
                else
                {
                    StatusMessage = $"{Allergens.Count} alergeni încărcați.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la încărcarea alergenilor: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void LoadLowStockDishes()
        {
            IsLoading = true;
            StatusMessage = "Se verifică stocul...";

            try
            {
                decimal thresholdQuantity = 5;


                var lowStockDishes = await _databaseService.GetDishesNearDepletionAsync(thresholdQuantity);
                LowStockDishes = new ObservableCollection<Dish>(lowStockDishes);

                if (LowStockDishes.Count == 0)
                {
                    StatusMessage = "Nu există preparate cu stoc redus.";
                }
                else
                {
                    StatusMessage = $"{LowStockDishes.Count} preparate cu stoc redus.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la verificarea stocului: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void LoadOrders()
        {
            IsLoading = true;
            StatusMessage = "Se încarcă comenzile...";

            try
            {
                var orders = await _databaseService.GetOrdersInDateRangeAsync(StartDate, EndDate);

                var sortedOrders = orders.OrderByDescending(o => o.OrderDate).ToList();

                AllOrders = new ObservableCollection<Order>(sortedOrders);

                var activeOrders = sortedOrders.Where(o =>
                    o.Status != "livrata" && o.Status != "anulata").ToList();

                ActiveOrders = new ObservableCollection<Order>(activeOrders);

                if (AllOrders.Count == 0)
                {
                    StatusMessage = "Nu există comenzi în intervalul specificat.";
                }
                else
                {
                    StatusMessage = $"{AllOrders.Count} comenzi în total și {ActiveOrders.Count} comenzi active.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la încărcarea comenzilor: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void RefreshData()
        {
            switch (SelectedTabIndex)
            {
                case 0: 
                    LoadCategories();
                    break;
                case 1: 
                    LoadDishes();
                    break;
                case 2: 
                    LoadMenus();
                    break;
                case 3: 
                    LoadAllergens();
                    break;
                case 4: 
                    LoadLowStockDishes();
                    break;
                case 5: 
                    LoadOrders();
                    break;
            }
        }
        #endregion

        #region CRUD Operations

        private async void SaveCategory()
        {
            if (string.IsNullOrWhiteSpace(EditCategory.Name))
            {
                MessageBox.Show("Numele categoriei este obligatoriu.", "Validare",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;

            try
            {
                bool success;

                if (EditCategory.CategoryId > 0)
                {
                    success = await _databaseService.UpdateCategoryAsync(EditCategory);

                    if (success)
                    {
                        StatusMessage = "Categoria a fost actualizată cu succes.";

                        var index = Categories.IndexOf(Categories.FirstOrDefault(c => c.CategoryId == EditCategory.CategoryId));
                        if (index >= 0)
                        {
                            Categories[index] = new Category
                            {
                                CategoryId = EditCategory.CategoryId,
                                Name = EditCategory.Name,
                                Description = EditCategory.Description
                            };
                        }
                    }
                    else
                    {
                        StatusMessage = "Nu s-a putut actualiza categoria.";
                    }
                }
                else
                {
                    int categoryId = await _databaseService.CreateCategoryAsync(EditCategory);

                    if (categoryId > 0)
                    {
                        StatusMessage = "Categoria a fost adăugată cu succes.";

                        Categories.Add(new Category
                        {
                            CategoryId = categoryId,
                            Name = EditCategory.Name,
                            Description = EditCategory.Description
                        });

                        EditCategory = new Category();
                    }
                    else
                    {
                        StatusMessage = "Nu s-a putut adăuga categoria.";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la salvarea categoriei: {ex.Message}";
                MessageBox.Show($"Eroare la salvarea categoriei: {ex.Message}", "Eroare",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void DeleteCategory()
        {
            if (SelectedCategory == null)
                return;

            var result = MessageBox.Show($"Sigur doriți să ștergeți categoria '{SelectedCategory.Name}'?",
                                        "Confirmare ștergere", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            IsLoading = true;

            try
            {
                bool success = await _databaseService.DeleteCategoryAsync(SelectedCategory.CategoryId);

                if (success)
                {
                    StatusMessage = "Categoria a fost ștearsă cu succes.";

                    Categories.Remove(SelectedCategory);
                    SelectedCategory = null;
                }
                else
                {
                    StatusMessage = "Nu s-a putut șterge categoria.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la ștergerea categoriei: {ex.Message}";
                MessageBox.Show($"Eroare la ștergerea categoriei: {ex.Message}", "Eroare",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void NewCategory()
        {
            EditCategory = new Category();
            SelectedCategory = null;
        }

        public ICommand BrowseImageCommand { get; }


        private async void SaveDish()
        {
            if (string.IsNullOrWhiteSpace(EditDish.Name))
            {
                MessageBox.Show("Numele preparatului este obligatoriu.", "Validare",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (EditDish.Price <= 0)
            {
                MessageBox.Show("Prețul trebuie să fie mai mare decât zero.", "Validare",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (EditDish.PortionQuantity <= 0)
            {
                MessageBox.Show("Cantitatea per porție trebuie să fie mai mare decât zero.", "Validare",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (EditDish.CategoryId <= 0)
            {
                MessageBox.Show("Trebuie selectată o categorie.", "Validare",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!string.IsNullOrEmpty(EditDish.ImagePath))
            {
                try
                {
                    if (!EditDish.ImagePath.StartsWith("/MenuApp;component/Images/") &&
                        !EditDish.ImagePath.StartsWith("pack://application:,,,/MenuApp;component/Images/"))
                    {
                        string fileName = System.IO.Path.GetFileName(EditDish.ImagePath);

                        string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                        string imagesFolder = System.IO.Path.Combine(appDirectory, "Images");

                        if (!System.IO.Directory.Exists(imagesFolder))
                        {
                            System.IO.Directory.CreateDirectory(imagesFolder);
                        }

                        string destinationPath = System.IO.Path.Combine(imagesFolder, fileName);

                        if (System.IO.File.Exists(EditDish.ImagePath) && !System.IO.File.Exists(destinationPath))
                        {
                            System.IO.File.Copy(EditDish.ImagePath, destinationPath);
                        }

                        EditDish.ImagePath = $"/MenuApp;component/Images/{fileName}";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Avertisment: Nu s-a putut procesa imaginea. {ex.Message}",
                                  "Avertisment", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            IsLoading = true;

            try
            {
                bool success;

                if (EditDish.DishId > 0)
                {
                    success = await _databaseService.UpdateDishAsync(EditDish);

                    if (success)
                    {
                        StatusMessage = "Preparatul a fost actualizat cu succes.";

                        var index = Dishes.IndexOf(Dishes.FirstOrDefault(d => d.DishId == EditDish.DishId));
                        if (index >= 0)
                        {
                            Dishes[index] = new Dish
                            {
                                DishId = EditDish.DishId,
                                Name = EditDish.Name,
                                Price = EditDish.Price,
                                PortionQuantity = EditDish.PortionQuantity,
                                TotalQuantity = EditDish.TotalQuantity,
                                CategoryId = EditDish.CategoryId,
                                Category = Categories.FirstOrDefault(c => c.CategoryId == EditDish.CategoryId),
                                ImagePath = EditDish.ImagePath,
                                Allergens = EditDish.Allergens
                            };
                        }
                    }
                    else
                    {
                        StatusMessage = "Nu s-a putut actualiza preparatul.";
                    }
                }
                else
                {
                    int dishId = await _databaseService.CreateDishAsync(EditDish);

                    if (dishId > 0)
                    {
                        StatusMessage = "Preparatul a fost adăugat cu succes.";

                        EditDish.DishId = dishId;
                        EditDish.Category = Categories.FirstOrDefault(c => c.CategoryId == EditDish.CategoryId);
                        Dishes.Add(EditDish);

                        // Resetăm formularul
                        EditDish = new Dish();
                    }
                    else
                    {
                        StatusMessage = "Nu s-a putut adăuga preparatul.";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la salvarea preparatului: {ex.Message}";
                MessageBox.Show($"Eroare la salvarea preparatului: {ex.Message}", "Eroare",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void BrowseImage()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Image files (*.jpg, *.jpeg, *.png, *.gif)|*.jpg;*.jpeg;*.png;*.gif";

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    string selectedFile = dlg.FileName;

                    EditDish.ImagePath = selectedFile;
                    OnPropertyChanged(nameof(EditDish));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Eroare la selectarea imaginii: {ex.Message}",
                                  "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private async void DeleteDish()
        {
            if (SelectedDish == null)
                return;

            var result = MessageBox.Show($"Sigur doriți să ștergeți preparatul '{SelectedDish.Name}'?",
                                        "Confirmare ștergere", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            IsLoading = true;

            try
            {
                bool success = await _databaseService.DeleteDishAsync(SelectedDish.DishId);

                if (success)
                {
                    StatusMessage = "Preparatul a fost șters cu succes.";

                    Dishes.Remove(SelectedDish);
                    SelectedDish = null;
                }
                else
                {
                    StatusMessage = "Nu s-a putut șterge preparatul.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la ștergerea preparatului: {ex.Message}";
                MessageBox.Show($"Eroare la ștergerea preparatului: {ex.Message}", "Eroare",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void NewDish()
        {
            EditDish = new Dish();
            SelectedDish = null;
        }

        private async void SaveMenu()
        {
            if (string.IsNullOrWhiteSpace(EditMenu.Name))
            {
                MessageBox.Show("Numele meniului este obligatoriu.", "Validare",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (EditMenu.CategoryId <= 0)
            {
                MessageBox.Show("Trebuie selectată o categorie.", "Validare",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;

            try
            {
                bool success;

                if (EditMenu.MenuId > 0)
                {
                    success = await _databaseService.UpdateMenuAsync(EditMenu);

                    if (success)
                    {
                        StatusMessage = "Meniul a fost actualizat cu succes.";

                        var index = Menus.IndexOf(Menus.FirstOrDefault(m => m.MenuId == EditMenu.MenuId));
                        if (index >= 0)
                        {
                            Menus[index] = new Menu
                            {
                                MenuId = EditMenu.MenuId,
                                Name = EditMenu.Name,
                                CategoryId = EditMenu.CategoryId,
                                Price = EditMenu.Price,
                                IsAvailable = EditMenu.IsAvailable,
                                MenuDishes = EditMenu.MenuDishes
                            };
                        }
                    }
                    else
                    {
                        StatusMessage = "Nu s-a putut actualiza meniul.";
                    }
                }
                else
                {
                    int menuId = await _databaseService.CreateMenuAsync(EditMenu);

                    if (menuId > 0)
                    {
                        StatusMessage = "Meniul a fost adăugat cu succes.";

                        EditMenu.MenuId = menuId;
                        Menus.Add(EditMenu);

                        EditMenu = new Menu();
                    }
                    else
                    {
                        StatusMessage = "Nu s-a putut adăuga meniul.";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la salvarea meniului: {ex.Message}";
                MessageBox.Show($"Eroare la salvarea meniului: {ex.Message}", "Eroare",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void DeleteMenu()
        {
            if (SelectedMenu == null)
                return;

            var result = MessageBox.Show($"Sigur doriți să ștergeți meniul '{SelectedMenu.Name}'?",
                                        "Confirmare ștergere", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            IsLoading = true;

            try
            {
                bool success = await _databaseService.DeleteMenuAsync(SelectedMenu.MenuId);

                if (success)
                {
                    StatusMessage = "Meniul a fost șters cu succes.";

                    Menus.Remove(SelectedMenu);
                    SelectedMenu = null;
                }
                else
                {
                    StatusMessage = "Nu s-a putut șterge meniul.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la ștergerea meniului: {ex.Message}";
                MessageBox.Show($"Eroare la ștergerea meniului: {ex.Message}", "Eroare",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void NewMenu()
        {
            EditMenu = new Menu();
            SelectedMenu = null;
        }

        private async void SaveAllergen()
        {
            if (string.IsNullOrWhiteSpace(EditAllergen.Name))
            {
                MessageBox.Show("Numele alergenului este obligatoriu.", "Validare",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;

            try
            {
                bool success;

                if (EditAllergen.AllergenId > 0)
                {
                    success = await _databaseService.UpdateAllergenAsync(EditAllergen);

                    if (success)
                    {
                        StatusMessage = "Alergenul a fost actualizat cu succes.";

                        var index = Allergens.IndexOf(Allergens.FirstOrDefault(a => a.AllergenId == EditAllergen.AllergenId));
                        if (index >= 0)
                        {
                            Allergens[index] = new Allergen
                            {
                                AllergenId = EditAllergen.AllergenId,
                                Name = EditAllergen.Name,
                                Description = EditAllergen.Description
                            };
                        }
                    }
                    else
                    {
                        StatusMessage = "Nu s-a putut actualiza alergenul.";
                    }
                }
                else
                {
                    int allergenId = await _databaseService.CreateAllergenAsync(EditAllergen);

                    if (allergenId > 0)
                    {
                        StatusMessage = "Alergenul a fost adăugat cu succes.";

                        EditAllergen.AllergenId = allergenId;
                        Allergens.Add(EditAllergen);

                        EditAllergen = new Allergen();
                    }
                    else
                    {
                        StatusMessage = "Nu s-a putut adăuga alergenul.";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la salvarea alergenului: {ex.Message}";
                MessageBox.Show($"Eroare la salvarea alergenului: {ex.Message}", "Eroare",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void DeleteAllergen()
        {
            if (SelectedAllergen == null)
                return;

            var result = MessageBox.Show($"Sigur doriți să ștergeți alergenul '{SelectedAllergen.Name}'?",
                                        "Confirmare ștergere", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            IsLoading = true;

            try
            {
                bool success = await _databaseService.DeleteAllergenAsync(SelectedAllergen.AllergenId);

                if (success)
                {
                    StatusMessage = "Alergenul a fost șters cu succes.";

                    Allergens.Remove(SelectedAllergen);
                    SelectedAllergen = null;
                }
                else
                {
                    StatusMessage = "Nu s-a putut șterge alergenul.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la ștergerea alergenului: {ex.Message}";
                MessageBox.Show($"Eroare la ștergerea alergenului: {ex.Message}", "Eroare",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void NewAllergen()
        {
            EditAllergen = new Allergen();
            SelectedAllergen = null;
        }


        private async void ChangeOrderStatus()
{
    if (SelectedOrder == null) return;
    
    string currentStatus = SelectedOrder.Status;
    string newStatus = "";
    
    switch (currentStatus)
    {
        case "inregistrata": newStatus = "se pregateste"; break;
        case "se pregateste": newStatus = "a plecat la client"; break;
        case "a plecat la client": newStatus = "livrata"; break;
        default: return;
    }
    
    bool success = await _databaseService.UpdateOrderStatusAsync(SelectedOrder.OrderId, newStatus);
    if (success)
    {
        LoadOrders();
    }
}

private async void CancelOrder()
{
    if (SelectedOrder == null) return;
    
    if (SelectedOrder.Status == "livrata" || SelectedOrder.Status == "anulata")
        return;
    
    bool success = await _databaseService.CancelOrderAsync(SelectedOrder.OrderId);
    if (success)
    {
        LoadOrders();
    }
}
        #endregion
    }
}