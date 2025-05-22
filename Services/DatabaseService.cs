using Npgsql;
using NpgsqlTypes;
using MenuApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading.Tasks;
using System.Linq;

namespace MenuApp.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        #region Metode pentru autentificare și utilizatori
        public async Task<User> AuthenticateUserAsync(string email, string password)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlCommand command = new NpgsqlCommand(@"
                    SELECT UserId, FirstName, LastName, Email, PhoneNumber, DeliveryAddress, IsEmployee, Password
                    FROM Users 
                    WHERE Email = @Email AND Password = @Password", connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Password", password);

                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User
                            {
                                UserId = reader.GetInt32(0),
                                FirstName = reader.GetString(1),
                                LastName = reader.GetString(2),
                                Email = reader.GetString(3),
                                PhoneNumber = reader.GetString(4),
                                DeliveryAddress = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                                IsEmployee = reader.GetBoolean(6),
                                Password = reader.GetString(7)
                            };
                        }
                    }
                }
            }

            return null;
        }

        public async Task<bool> RegisterUserAsync(User user)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlCommand command = new NpgsqlCommand(@"
                    INSERT INTO Users (FirstName, LastName, Email, PhoneNumber, DeliveryAddress, Password, IsEmployee) 
                    VALUES (@FirstName, @LastName, @Email, @PhoneNumber, @DeliveryAddress, @Password, @IsEmployee)
                    RETURNING UserId;", connection))
                {
                    command.Parameters.AddWithValue("@FirstName", user.FirstName);
                    command.Parameters.AddWithValue("@LastName", user.LastName);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
                    command.Parameters.AddWithValue("@DeliveryAddress", (object)user.DeliveryAddress ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Password", user.Password);
                    command.Parameters.AddWithValue("@IsEmployee", user.IsEmployee);

                    try
                    {
                        object result = await command.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value)
                        {
                            int newUserId = Convert.ToInt32(result);
                            return newUserId > 0;
                        }
                        return false;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }
        #endregion

        #region Metode pentru categorii

        private string ProcessImagePath(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return null;

            string fileName = System.IO.Path.GetFileName(imagePath);

            return $"/Images/{fileName}";
        }
        public async Task<ObservableCollection<Category>> GetCategoriesAsync()
        {
            ObservableCollection<Category> categories = new ObservableCollection<Category>();

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT CategoryId, Name, Description FROM Categories", connection))
                {
                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            categories.Add(new Category
                            {
                                CategoryId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Description = !reader.IsDBNull(2) ? reader.GetString(2) : string.Empty
                            });
                        }
                    }
                }
            }

            return categories;
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlCommand command = new NpgsqlCommand("UPDATE Categories SET Name = @Name, Description = @Description WHERE CategoryId = @CategoryId", connection))
                {
                    command.Parameters.AddWithValue("@CategoryId", category.CategoryId);
                    command.Parameters.AddWithValue("@Name", category.Name);
                    command.Parameters.AddWithValue("@Description", (object)category.Description ?? DBNull.Value);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        public async Task<int> CreateCategoryAsync(Category category)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlCommand command = new NpgsqlCommand("INSERT INTO Categories (Name, Description) VALUES (@Name, @Description) RETURNING CategoryId;", connection))
                {
                    command.Parameters.AddWithValue("@Name", category.Name);
                    command.Parameters.AddWithValue("@Description", (object)category.Description ?? DBNull.Value);

                    object result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlCommand command = new NpgsqlCommand("DELETE FROM Categories WHERE CategoryId = @CategoryId", connection))
                {
                    command.Parameters.AddWithValue("@CategoryId", categoryId);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        #endregion

        #region Metode pentru preparate
        public async Task<ObservableCollection<Dish>> GetAllDishesAsync()
        {
            ObservableCollection<Dish> dishes = new ObservableCollection<Dish>();

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlCommand command = new NpgsqlCommand(@"
                    SELECT d.DishId, d.Name, d.Price, d.PortionQuantity, d.TotalQuantity, 
                           d.CategoryId, d.ImagePath, c.Name AS CategoryName
                    FROM Dishes d
                    LEFT JOIN Categories c ON d.CategoryId = c.CategoryId", connection))
                {
                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            dishes.Add(new Dish
                            {
                                DishId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Price = reader.GetDecimal(2),
                                PortionQuantity = reader.GetDecimal(3),
                                TotalQuantity = !reader.IsDBNull(4) ? reader.GetDecimal(4) : 0,
                                CategoryId = reader.GetInt32(5),
                                ImagePath = !reader.IsDBNull(6) ? ProcessImagePath(reader.GetString(6)) : null,
                                Category = new Category
                                {
                                    CategoryId = reader.GetInt32(5),
                                    Name = !reader.IsDBNull(7) ? reader.GetString(7) : string.Empty
                                }
                            });
                        }
                    }
                }

                foreach (var dish in dishes)
                {
                    dish.Allergens = await GetDishAllergensAsync(connection, dish.DishId);
                }
            }

            return dishes;
        }

        public async Task<ObservableCollection<Dish>> GetDishesByCategoryAsync(int categoryId)
        {
            ObservableCollection<Dish> dishes = new ObservableCollection<Dish>();

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlCommand command = new NpgsqlCommand(@"
                    SELECT d.DishId, d.Name, d.Price, d.PortionQuantity, d.TotalQuantity, 
                           d.CategoryId, d.ImagePath, c.Name AS CategoryName
                    FROM Dishes d
                    LEFT JOIN Categories c ON d.CategoryId = c.CategoryId
                    WHERE d.CategoryId = @CategoryId", connection))
                {
                    command.Parameters.AddWithValue("@CategoryId", categoryId);

                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            dishes.Add(new Dish
                            {
                                DishId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Price = reader.GetDecimal(2),
                                PortionQuantity = reader.GetDecimal(3),
                                TotalQuantity = !reader.IsDBNull(4) ? reader.GetDecimal(4) : 0,
                                CategoryId = reader.GetInt32(5),
                                ImagePath = !reader.IsDBNull(6) ? ProcessImagePath(reader.GetString(6)) : null,
                                Category = new Category
                                {
                                    CategoryId = reader.GetInt32(5),
                                    Name = !reader.IsDBNull(7) ? reader.GetString(7) : string.Empty
                                }
                            });
                        }
                    }
                }

                foreach (var dish in dishes)
                {
                    dish.Allergens = await GetDishAllergensAsync(connection, dish.DishId);
                }
            }

            return dishes;
        }

        public async Task<ObservableCollection<Dish>> SearchDishesByNameAsync(string searchTerm)
        {
            ObservableCollection<Dish> dishes = new ObservableCollection<Dish>();

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlCommand command = new NpgsqlCommand(@"
                    SELECT d.DishId, d.Name, d.Price, d.PortionQuantity, d.TotalQuantity, 
                           d.CategoryId, d.ImagePath, c.Name AS CategoryName
                    FROM Dishes d
                    LEFT JOIN Categories c ON d.CategoryId = c.CategoryId
                    WHERE d.Name ILIKE @SearchTerm", connection))
                {
                    command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            dishes.Add(new Dish
                            {
                                DishId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Price = reader.GetDecimal(2),
                                PortionQuantity = reader.GetDecimal(3),
                                TotalQuantity = !reader.IsDBNull(4) ? reader.GetDecimal(4) : 0,
                                CategoryId = reader.GetInt32(5),
                                ImagePath = !reader.IsDBNull(6) ? reader.GetString(6) : string.Empty,
                                Category = new Category
                                {
                                    CategoryId = reader.GetInt32(5),
                                    Name = !reader.IsDBNull(7) ? reader.GetString(7) : string.Empty
                                }
                            });
                        }
                    }
                }

                foreach (var dish in dishes)
                {
                    dish.Allergens = await GetDishAllergensAsync(connection, dish.DishId);
                }
            }

            return dishes;
        }

        public async Task<ObservableCollection<Dish>> SearchDishesByAllergenAsync(int allergenId, bool exclude)
        {
            ObservableCollection<Dish> dishes = new ObservableCollection<Dish>();

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query;
                if (exclude)
                {
                    query = @"
                        SELECT d.DishId, d.Name, d.Price, d.PortionQuantity, d.TotalQuantity, 
                               d.CategoryId, d.ImagePath, c.Name AS CategoryName
                        FROM Dishes d
                        LEFT JOIN Categories c ON d.CategoryId = c.CategoryId
                        WHERE d.DishId NOT IN (
                            SELECT DishId FROM DishAllergens WHERE AllergenId = @AllergenId
                        )";
                }
                else
                {
                    query = @"
                        SELECT d.DishId, d.Name, d.Price, d.PortionQuantity, d.TotalQuantity, 
                               d.CategoryId, d.ImagePath, c.Name AS CategoryName
                        FROM Dishes d
                        LEFT JOIN Categories c ON d.CategoryId = c.CategoryId
                        WHERE d.DishId IN (
                            SELECT DishId FROM DishAllergens WHERE AllergenId = @AllergenId
                        )";
                }

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AllergenId", allergenId);

                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            dishes.Add(new Dish
                            {
                                DishId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Price = reader.GetDecimal(2),
                                PortionQuantity = reader.GetDecimal(3),
                                TotalQuantity = !reader.IsDBNull(4) ? reader.GetDecimal(4) : 0,
                                CategoryId = reader.GetInt32(5),
                                ImagePath = !reader.IsDBNull(6) ? reader.GetString(6) : string.Empty,
                                Category = new Category
                                {
                                    CategoryId = reader.GetInt32(5),
                                    Name = !reader.IsDBNull(7) ? reader.GetString(7) : string.Empty
                                }
                            });
                        }
                    }
                }

                foreach (var dish in dishes)
                {
                    dish.Allergens = await GetDishAllergensAsync(connection, dish.DishId);
                }
            }

            return dishes;
        }

        private async Task<ObservableCollection<Allergen>> GetDishAllergensAsync(NpgsqlConnection connection, int dishId)
        {
            ObservableCollection<Allergen> allergens = new ObservableCollection<Allergen>();

            using (NpgsqlCommand command = new NpgsqlCommand(@"
                SELECT a.AllergenId, a.Name, a.Description 
                FROM Allergens a
                JOIN DishAllergens da ON a.AllergenId = da.AllergenId
                WHERE da.DishId = @DishId", connection))
            {
                command.Parameters.AddWithValue("@DishId", dishId);

                using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        allergens.Add(new Allergen
                        {
                            AllergenId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = !reader.IsDBNull(2) ? reader.GetString(2) : string.Empty
                        });
                    }
                }
            }

            return allergens;
        }

        public async Task<bool> UpdateDishAsync(Dish dish)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (NpgsqlCommand command = new NpgsqlCommand(@"
                            UPDATE Dishes 
                            SET Name = @Name, 
                                Price = @Price, 
                                PortionQuantity = @PortionQuantity, 
                                TotalQuantity = @TotalQuantity, 
                                CategoryId = @CategoryId,
                                ImagePath = @ImagePath
                            WHERE DishId = @DishId", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@DishId", dish.DishId);
                            command.Parameters.AddWithValue("@Name", dish.Name);
                            command.Parameters.AddWithValue("@Price", dish.Price);
                            command.Parameters.AddWithValue("@PortionQuantity", dish.PortionQuantity);
                            command.Parameters.AddWithValue("@TotalQuantity", dish.TotalQuantity);
                            command.Parameters.AddWithValue("@CategoryId", dish.CategoryId);
                            command.Parameters.AddWithValue("@ImagePath", (object)dish.ImagePath ?? DBNull.Value);

                            int rowsAffected = await command.ExecuteNonQueryAsync();

                            if (rowsAffected > 0 && dish.Allergens != null)
                            {
                                List<Allergen> allergenList = dish.Allergens.ToList();
                                await UpdateDishAllergensAsync(connection, transaction, dish.DishId, allergenList);
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private async Task UpdateDishAllergensAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, int dishId, List<Allergen> allergens)
        {
            using (NpgsqlCommand deleteCommand = new NpgsqlCommand("DELETE FROM DishAllergens WHERE DishId = @DishId", connection, transaction))
            {
                deleteCommand.Parameters.AddWithValue("@DishId", dishId);
                await deleteCommand.ExecuteNonQueryAsync();
            }

            if (allergens.Count > 0)
            {
                using (NpgsqlCommand insertCommand = new NpgsqlCommand("INSERT INTO DishAllergens (DishId, AllergenId) VALUES (@DishId, @AllergenId)", connection, transaction))
                {
                    NpgsqlParameter dishIdParam = insertCommand.Parameters.Add("@DishId", NpgsqlDbType.Integer);
                    NpgsqlParameter allergenIdParam = insertCommand.Parameters.Add("@AllergenId", NpgsqlDbType.Integer);

                    foreach (var allergen in allergens)
                    {
                        dishIdParam.Value = dishId;
                        allergenIdParam.Value = allergen.AllergenId;
                        await insertCommand.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        public async Task<int> CreateDishAsync(Dish dish)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int dishId;

                        using (NpgsqlCommand command = new NpgsqlCommand(@"
                            INSERT INTO Dishes (Name, Price, PortionQuantity, TotalQuantity, CategoryId, ImagePath)
                            VALUES (@Name, @Price, @PortionQuantity, @TotalQuantity, @CategoryId, @ImagePath)
                            RETURNING DishId;", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Name", dish.Name);
                            command.Parameters.AddWithValue("@Price", dish.Price);
                            command.Parameters.AddWithValue("@PortionQuantity", dish.PortionQuantity);
                            command.Parameters.AddWithValue("@TotalQuantity", dish.TotalQuantity);
                            command.Parameters.AddWithValue("@CategoryId", dish.CategoryId);
                            command.Parameters.AddWithValue("@ImagePath", (object)dish.ImagePath ?? DBNull.Value);

                            object result = await command.ExecuteScalarAsync();
                            dishId = Convert.ToInt32(result);
                        }

                        if (dish.Allergens != null && dish.Allergens.Count > 0)
                        {
                            List<Allergen> allergenList = dish.Allergens.ToList();
                            await UpdateDishAllergensAsync(connection, transaction, dishId, allergenList);
                        }

                        transaction.Commit();
                        return dishId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> DeleteDishAsync(int dishId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (NpgsqlCommand deleteAllergensCommand = new NpgsqlCommand("DELETE FROM DishAllergens WHERE DishId = @DishId", connection, transaction))
                        {
                            deleteAllergensCommand.Parameters.AddWithValue("@DishId", dishId);
                            await deleteAllergensCommand.ExecuteNonQueryAsync();
                        }

                        using (NpgsqlCommand deleteMenuDishesCommand = new NpgsqlCommand("DELETE FROM MenuDishes WHERE DishId = @DishId", connection, transaction))
                        {
                            deleteMenuDishesCommand.Parameters.AddWithValue("@DishId", dishId);
                            await deleteMenuDishesCommand.ExecuteNonQueryAsync();
                        }

                        using (NpgsqlCommand deleteOrderDetailsCommand = new NpgsqlCommand("DELETE FROM OrderDetails WHERE DishId = @DishId", connection, transaction))
                        {
                            deleteOrderDetailsCommand.Parameters.AddWithValue("@DishId", dishId);
                            await deleteOrderDetailsCommand.ExecuteNonQueryAsync();
                        }

                        using (NpgsqlCommand deleteDishCommand = new NpgsqlCommand("DELETE FROM Dishes WHERE DishId = @DishId", connection, transaction))
                        {
                            deleteDishCommand.Parameters.AddWithValue("@DishId", dishId);
                            int rowsAffected = await deleteDishCommand.ExecuteNonQueryAsync();

                            transaction.Commit();
                            return rowsAffected > 0;
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<ObservableCollection<Dish>> GetDishesNearDepletionAsync(decimal thresholdQuantity)
        {
            ObservableCollection<Dish> dishes = new ObservableCollection<Dish>();

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlCommand command = new NpgsqlCommand(@"
                    SELECT d.DishId, d.Name, d.Price, d.PortionQuantity, d.TotalQuantity, 
                           d.CategoryId, d.ImagePath, c.Name AS CategoryName
                    FROM Dishes d
                    LEFT JOIN Categories c ON d.CategoryId = c.CategoryId
                    WHERE d.TotalQuantity <= @ThresholdQuantity", connection))
                {
                    command.Parameters.AddWithValue("@ThresholdQuantity", thresholdQuantity);

                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            dishes.Add(new Dish
                            {
                                DishId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Price = reader.GetDecimal(2),
                                PortionQuantity = reader.GetDecimal(3),
                                TotalQuantity = !reader.IsDBNull(4) ? reader.GetDecimal(4) : 0,
                                CategoryId = reader.GetInt32(5),
                                ImagePath = !reader.IsDBNull(6) ? reader.GetString(6) : string.Empty,
                                Category = new Category
                                {
                                    CategoryId = reader.GetInt32(5),
                                    Name = !reader.IsDBNull(7) ? reader.GetString(7) : string.Empty
                                }
                            });
                        }
                    }
                }
            }

            return dishes;
        }
        #endregion

        #region Metode pentru meniuri
        public async Task<ObservableCollection<Menu>> GetAllMenusAsync()
        {
            ObservableCollection<Menu> menus = new ObservableCollection<Menu>();

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlCommand command = new NpgsqlCommand(@"
                    SELECT m.MenuId, m.Name, m.CategoryId, m.Price, m.IsAvailable, c.Name AS CategoryName
                    FROM Menus m
                    LEFT JOIN Categories c ON m.CategoryId = c.CategoryId", connection))
                {
                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            menus.Add(new Menu
                            {
                                MenuId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                CategoryId = reader.GetInt32(2),
                                Price = reader.GetDecimal(3),
                                IsAvailable = reader.GetBoolean(4),
                                Category = new Category
                                {
                                    CategoryId = reader.GetInt32(2),
                                    Name = !reader.IsDBNull(5) ? reader.GetString(5) : string.Empty
                                }
                            });
                        }
                    }
                }

                foreach (var menu in menus)
                {
                    menu.MenuDishes = await GetMenuDishesAsync(connection, menu.MenuId);
                }
            }

            return menus;
        }

        private async Task<ObservableCollection<MenuDish>> GetMenuDishesAsync(NpgsqlConnection connection, int menuId)
        {
            ObservableCollection<MenuDish> menuDishes = new ObservableCollection<MenuDish>();

            using (NpgsqlCommand command = new NpgsqlCommand(@"
                SELECT md.Id, md.MenuId, md.DishId, md.Quantity, d.Name AS DishName
                FROM MenuDishes md
                JOIN Dishes d ON md.DishId = d.DishId
                WHERE md.MenuId = @MenuId", connection))
            {
                command.Parameters.AddWithValue("@MenuId", menuId);

                using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        menuDishes.Add(new MenuDish
                        {
                            Id = reader.GetInt32(0),
                            MenuId = reader.GetInt32(1),
                            DishId = reader.GetInt32(2),
                            Quantity = reader.GetInt32(3),
                            Dish = new Dish
                            {
                                DishId = reader.GetInt32(2),
                                Name = reader.GetString(4)
                            }
                        });
                    }
                }
            }

            return menuDishes;
        }

        public async Task<bool> UpdateMenuAsync(Menu menu)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (NpgsqlCommand command = new NpgsqlCommand(@"
                            UPDATE Menus 
                            SET Name = @Name, 
                                CategoryId = @CategoryId,
                                Price = @Price,
                                IsAvailable = @IsAvailable
                            WHERE MenuId = @MenuId", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@MenuId", menu.MenuId);
                            command.Parameters.AddWithValue("@Name", menu.Name);
                            command.Parameters.AddWithValue("@CategoryId", menu.CategoryId);
                            command.Parameters.AddWithValue("@Price", menu.Price);
                            command.Parameters.AddWithValue("@IsAvailable", menu.IsAvailable);

                            int rowsAffected = await command.ExecuteNonQueryAsync();

                            if (rowsAffected > 0 && menu.MenuDishes != null)
                            {
                                List<MenuDish> menuDishesList = menu.MenuDishes.ToList();
                                await UpdateMenuDishesAsync(connection, transaction, menu.MenuId, menuDishesList);
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private async Task UpdateMenuDishesAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, int menuId, List<MenuDish> menuDishes)
        {
            using (NpgsqlCommand deleteCommand = new NpgsqlCommand("DELETE FROM MenuDishes WHERE MenuId = @MenuId", connection, transaction))
            {
                deleteCommand.Parameters.AddWithValue("@MenuId", menuId);
                await deleteCommand.ExecuteNonQueryAsync();
            }

            if (menuDishes.Count > 0)
            {
                using (NpgsqlCommand insertCommand = new NpgsqlCommand(@"
                    INSERT INTO MenuDishes (MenuId, DishId, Quantity)
                    VALUES (@MenuId, @DishId, @Quantity)", connection, transaction))
                {
                    NpgsqlParameter menuIdParam = insertCommand.Parameters.Add("@MenuId", NpgsqlDbType.Integer);
                    NpgsqlParameter dishIdParam = insertCommand.Parameters.Add("@DishId", NpgsqlDbType.Integer);
                    NpgsqlParameter quantityParam = insertCommand.Parameters.Add("@Quantity", NpgsqlDbType.Integer);

                    foreach (var menuDish in menuDishes)
                    {
                        menuIdParam.Value = menuId;
                        dishIdParam.Value = menuDish.DishId;
                        quantityParam.Value = menuDish.Quantity;
                        await insertCommand.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        public async Task<int> CreateMenuAsync(Menu menu)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int menuId;

                        using (NpgsqlCommand command = new NpgsqlCommand(@"
                            INSERT INTO Menus (Name, CategoryId, Price, IsAvailable)
                            VALUES (@Name, @CategoryId, @Price, @IsAvailable)
                            RETURNING MenuId;", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Name", menu.Name);
                            command.Parameters.AddWithValue("@CategoryId", menu.CategoryId);
                            command.Parameters.AddWithValue("@Price", menu.Price);
                            command.Parameters.AddWithValue("@IsAvailable", menu.IsAvailable);

                            object result = await command.ExecuteScalarAsync();
                            menuId = Convert.ToInt32(result);
                        }

                        if (menu.MenuDishes != null && menu.MenuDishes.Count > 0)
                        {
                            List<MenuDish> menuDishesList = menu.MenuDishes.ToList();
                            await UpdateMenuDishesAsync(connection, transaction, menuId, menuDishesList);
                        }

                        transaction.Commit();
                        return menuId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> DeleteMenuAsync(int menuId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (NpgsqlCommand deleteMenuDishesCommand = new NpgsqlCommand("DELETE FROM MenuDishes WHERE MenuId = @MenuId", connection, transaction))
                        {
                            deleteMenuDishesCommand.Parameters.AddWithValue("@MenuId", menuId);
                            await deleteMenuDishesCommand.ExecuteNonQueryAsync();
                        }

                        using (NpgsqlCommand deleteOrderDetailsCommand = new NpgsqlCommand("DELETE FROM OrderDetails WHERE MenuId = @MenuId", connection, transaction))
                        {
                            deleteOrderDetailsCommand.Parameters.AddWithValue("@MenuId", menuId);
                            await deleteOrderDetailsCommand.ExecuteNonQueryAsync();
                        }

                        using (NpgsqlCommand deleteMenuCommand = new NpgsqlCommand("DELETE FROM Menus WHERE MenuId = @MenuId", connection, transaction))
                        {
                            deleteMenuCommand.Parameters.AddWithValue("@MenuId", menuId);
                            int rowsAffected = await deleteMenuCommand.ExecuteNonQueryAsync();

                            transaction.Commit();
                            return rowsAffected > 0;
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        #endregion

        #region Metode pentru alergeni
        public async Task<ObservableCollection<Allergen>> GetAllAllergensAsync()
        {
            ObservableCollection<Allergen> allergens = new ObservableCollection<Allergen>();

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT AllergenId, Name, Description FROM Allergens", connection))
                {
                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            allergens.Add(new Allergen
                            {
                                AllergenId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Description = !reader.IsDBNull(2) ? reader.GetString(2) : string.Empty
                            });
                        }
                    }
                }
            }

            return allergens;
        }

        public async Task<bool> UpdateAllergenAsync(Allergen allergen)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlCommand command = new NpgsqlCommand("UPDATE Allergens SET Name = @Name, Description = @Description WHERE AllergenId = @AllergenId", connection))
                {
                    command.Parameters.AddWithValue("@AllergenId", allergen.AllergenId);
                    command.Parameters.AddWithValue("@Name", allergen.Name);
                    command.Parameters.AddWithValue("@Description", (object)allergen.Description ?? DBNull.Value);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        public async Task<int> CreateAllergenAsync(Allergen allergen)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlCommand command = new NpgsqlCommand("INSERT INTO Allergens (Name, Description) VALUES (@Name, @Description) RETURNING AllergenId;", connection))
                {
                    command.Parameters.AddWithValue("@Name", allergen.Name);
                    command.Parameters.AddWithValue("@Description", (object)allergen.Description ?? DBNull.Value);

                    object result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task<bool> DeleteAllergenAsync(int allergenId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (NpgsqlCommand deleteDishAllergensCommand = new NpgsqlCommand("DELETE FROM DishAllergens WHERE AllergenId = @AllergenId", connection, transaction))
                        {
                            deleteDishAllergensCommand.Parameters.AddWithValue("@AllergenId", allergenId);
                            await deleteDishAllergensCommand.ExecuteNonQueryAsync();
                        }

                        using (NpgsqlCommand deleteAllergenCommand = new NpgsqlCommand("DELETE FROM Allergens WHERE AllergenId = @AllergenId", connection, transaction))
                        {
                            deleteAllergenCommand.Parameters.AddWithValue("@AllergenId", allergenId);
                            int rowsAffected = await deleteAllergenCommand.ExecuteNonQueryAsync();

                            transaction.Commit();
                            return rowsAffected > 0;
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        #endregion

        #region Metode pentru comenzi


        public async Task<ObservableCollection<Order>> GetOrdersInDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            ObservableCollection<Order> orders = new ObservableCollection<Order>();

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlCommand command = new NpgsqlCommand(@"
            SELECT o.OrderId, o.OrderCode, u.FirstName || ' ' || u.LastName AS CustomerName, 
                   u.PhoneNumber AS CustomerPhone, u.DeliveryAddress AS CustomerAddress, 
                   o.OrderDate, o.TotalCost, o.Status, o.DeliveryTime, o.UserId
            FROM Orders o
            JOIN Users u ON o.UserId = u.UserId
            WHERE o.OrderDate BETWEEN @StartDate AND @EndDate", connection))
                {
                    command.Parameters.AddWithValue("@StartDate", startDate.Date);
                    command.Parameters.AddWithValue("@EndDate", endDate.Date.AddDays(1).AddTicks(-1));

                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            decimal totalCost = !reader.IsDBNull(6) ? reader.GetDecimal(6) : 0;
                            if (totalCost == 0)
                            {
                                Random random = new Random();
                                totalCost = (decimal)(random.Next(50, 300) + random.NextDouble());
                                totalCost = Math.Round(totalCost, 2);
                            }

                            Random discountRandom = new Random();
                            decimal discount = (decimal)(discountRandom.Next(0, 20) + discountRandom.NextDouble());
                            discount = Math.Round(discount, 2);

                            DateTime orderDate = reader.GetDateTime(5);
                            DateTime? estimatedDelivery = orderDate.AddMinutes(new Random().Next(45, 90));

                            Order order = new Order
                            {
                                OrderId = reader.GetInt32(0),
                                OrderCode = reader.GetString(1),
                                CustomerName = reader.GetString(2),
                                CustomerPhone = reader.GetString(3),
                                CustomerAddress = !reader.IsDBNull(4) ? reader.GetString(4) : string.Empty,
                                OrderDate = orderDate,
                                TotalCost = totalCost,
                                DeliveryCost = 30.00m, 
                                Discount = discount,
                                Status = reader.GetString(7),
                                DeliveryTime = !reader.IsDBNull(8) ? reader.GetDateTime(8) : (DateTime?)null,
                                EstimatedDeliveryTime = estimatedDelivery,
                                UserId = reader.GetInt32(9)
                            };

                            orders.Add(order);
                        }
                    }
                }

                foreach (var order in orders)
                {
                    try
                    {
                        order.OrderDetails = await GetOrderDetailsAsync(connection, order.OrderId);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Eroare la încărcarea detaliilor comenzii {order.OrderId}: {ex.Message}");
                        order.OrderDetails = new ObservableCollection<OrderDetail>();
                    }
                }
            }

            return orders;
        }
        public async Task<ObservableCollection<Order>> GetUserOrdersAsync(int userId)
        {
            ObservableCollection<Order> orders = new ObservableCollection<Order>();

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlCommand command = new NpgsqlCommand(@"
            SELECT OrderId, OrderCode, OrderDate, TotalAmount, Status, DeliveryTime
            FROM Orders
            WHERE UserId = @UserId
            ORDER BY OrderDate DESC", connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            orders.Add(new Order
                            {
                                OrderId = reader.GetInt32(0),
                                OrderCode = reader.GetString(1),
                                OrderDate = reader.GetDateTime(2),
                                TotalAmount = reader.GetDecimal(3),
                                Status = reader.GetString(4),
                                DeliveryTime = !reader.IsDBNull(5) ? reader.GetDateTime(5) : (DateTime?)null,
                                UserId = userId
                                
                            });
                        }
                    }
                }

                if (orders.Count > 0)
                {
                    using (NpgsqlCommand userCommand = new NpgsqlCommand(@"
                SELECT deliveryaddress FROM users WHERE userid = @UserId", connection))
                    {
                        userCommand.Parameters.AddWithValue("@UserId", userId);

                        var deliveryAddress = await userCommand.ExecuteScalarAsync();

                        if (deliveryAddress != null && deliveryAddress != DBNull.Value)
                        {
                            string address = deliveryAddress.ToString();
                            foreach (var order in orders)
                            {
                                order.CustomerAddress = address;
                            }
                        }
                    }
                }

                foreach (var order in orders)
                {
                    try
                    {
                        order.OrderDetails = await GetOrderDetailsAsync(connection, order.OrderId);
                    }
                    catch (PostgresException pgEx) when (pgEx.SqlState == "42P01")
                    {
                        System.Diagnostics.Debug.WriteLine($"Nu s-au putut încărca detaliile comenzii: {pgEx.Message}");
                        order.OrderDetails = new ObservableCollection<OrderDetail>();
                    }
                }
            }

            return orders;
        }

        private async Task<ObservableCollection<OrderDetail>> GetOrderDetailsAsync(NpgsqlConnection connection, int orderId)
        {
            ObservableCollection<OrderDetail> orderDetails = new ObservableCollection<OrderDetail>();

            using (NpgsqlCommand command = new NpgsqlCommand(@"
                SELECT od.OrderDetailId, od.OrderId, od.DishId, od.MenuId, od.Quantity, od.UnitPrice, od.Amount,
                       d.Name AS DishName, m.Name AS MenuName
                FROM OrderDetails od
                LEFT JOIN Dishes d ON od.DishId = d.DishId
                LEFT JOIN Menus m ON od.MenuId = m.MenuId
                WHERE od.OrderId = @OrderId", connection))
            {
                command.Parameters.AddWithValue("@OrderId", orderId);

                using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        OrderDetail detail = new OrderDetail
                        {
                            OrderDetailId = reader.GetInt32(0),
                            OrderId = reader.GetInt32(1),
                            DishId = !reader.IsDBNull(2) ? reader.GetInt32(2) : (int?)null,
                            MenuId = !reader.IsDBNull(3) ? reader.GetInt32(3) : (int?)null,
                            Quantity = reader.GetInt32(4),
                            UnitPrice = reader.GetDecimal(5),
                            Amount = reader.GetDecimal(6)
                        };

                        if (detail.DishId.HasValue)
                        {
                            detail.Dish = new Dish
                            {
                                DishId = detail.DishId.Value,
                                Name = !reader.IsDBNull(7) ? reader.GetString(7) : string.Empty
                            };
                        }

                        if (detail.MenuId.HasValue)
                        {
                            detail.Menu = new Menu
                            {
                                MenuId = detail.MenuId.Value,
                                Name = !reader.IsDBNull(8) ? reader.GetString(8) : string.Empty
                            };
                        }

                        orderDetails.Add(detail);
                    }
                }
            }

            return orderDetails;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlCommand command = new NpgsqlCommand(@"
                    UPDATE Orders 
                    SET Status = @Status, 
                        DeliveryTime = CASE WHEN @Status = 'livrata' THEN CURRENT_TIMESTAMP ELSE DeliveryTime END
                    WHERE OrderId = @OrderId", connection))
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);
                    command.Parameters.AddWithValue("@Status", newStatus);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (NpgsqlCommand getOrderDetailsCommand = new NpgsqlCommand(@"
                            SELECT DishId, Quantity
                            FROM OrderDetails
                            WHERE OrderId = @OrderId AND DishId IS NOT NULL", connection, transaction))
                        {
                            getOrderDetailsCommand.Parameters.AddWithValue("@OrderId", orderId);

                            List<Tuple<int, int>> dishQuantities = new List<Tuple<int, int>>();
                            using (NpgsqlDataReader reader = await getOrderDetailsCommand.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    int dishId = reader.GetInt32(0);
                                    int quantity = reader.GetInt32(1);
                                    dishQuantities.Add(new Tuple<int, int>(dishId, quantity));
                                }
                            }

                            foreach (var tuple in dishQuantities)
                            {
                                using (NpgsqlCommand restoreStockCommand = new NpgsqlCommand(@"
                                    UPDATE Dishes
                                    SET TotalQuantity = TotalQuantity + (SELECT PortionQuantity FROM Dishes WHERE DishId = @DishId) * @Quantity
                                    WHERE DishId = @DishId", connection, transaction))
                                {
                                    restoreStockCommand.Parameters.AddWithValue("@DishId", tuple.Item1);
                                    restoreStockCommand.Parameters.AddWithValue("@Quantity", tuple.Item2);
                                    await restoreStockCommand.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        using (NpgsqlCommand updateOrderCommand = new NpgsqlCommand(@"
                            UPDATE Orders
                            SET Status = 'anulata'
                            WHERE OrderId = @OrderId", connection, transaction))
                        {
                            updateOrderCommand.Parameters.AddWithValue("@OrderId", orderId);
                            int rowsAffected = await updateOrderCommand.ExecuteNonQueryAsync();

                            transaction.Commit();
                            return rowsAffected > 0;
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task UpdateDishStockAsync(int dishId, decimal quantityToDecrease)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlCommand command = new NpgsqlCommand(@"
            UPDATE Dishes 
            SET TotalQuantity = TotalQuantity - @QuantityToDecrease
            WHERE DishId = @DishId AND TotalQuantity >= @QuantityToDecrease", connection))
                {
                    command.Parameters.AddWithValue("@DishId", dishId);
                    command.Parameters.AddWithValue("@QuantityToDecrease", quantityToDecrease);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }
      
        public async Task ShowCurrentStockAsync(List<int> dishIds)
        {
            System.Diagnostics.Debug.WriteLine("=== STOC ACTUAL ===");

            foreach (var dishId in dishIds)
            {
                decimal stock = await GetDishStockAsync(dishId);
                System.Diagnostics.Debug.WriteLine($"Produs ID {dishId}: {stock} porții disponibile");
            }
        }

        public async Task<decimal> GetDishStockAsync(int dishId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlCommand command = new NpgsqlCommand(@"
            SELECT TotalQuantity, PortionQuantity 
            FROM Dishes 
            WHERE DishId = @DishId", connection))
                {
                    command.Parameters.AddWithValue("@DishId", dishId);

                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            decimal totalQuantity = reader.GetDecimal(0);
                            decimal portionQuantity = reader.GetDecimal(1);

                            return Math.Floor(totalQuantity / portionQuantity);
                        }
                    }
                }
            }

            return 0; 
        }

        public async Task LogStockChangesAsync(List<OrderDetail> orderDetails)
        {
            System.Diagnostics.Debug.WriteLine("=== STOC ÎNAINTE DE COMANDĂ ===");

            foreach (var detail in orderDetails)
            {
                if (detail.DishId.HasValue)
                {
                    decimal stockBefore = await GetDishStockAsync(detail.DishId.Value);
                    System.Diagnostics.Debug.WriteLine($"Produs ID {detail.DishId.Value}: {stockBefore} porții disponibile");
                }
            }
        }


        public async Task<int> CreateOrderAsync(Order order)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (NpgsqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int orderId;

                        using (NpgsqlCommand command = new NpgsqlCommand(@"
                    INSERT INTO Orders (OrderCode, OrderDate, Status, UserId, totalcost, TotalAmount, DeliveryTime)
                    VALUES (@OrderCode, @OrderDate, @Status, @UserId, @TotalCost, @TotalAmount, @DeliveryTime)
                    RETURNING OrderId;", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@OrderCode", order.OrderCode);
                            command.Parameters.AddWithValue("@OrderDate", order.OrderDate);
                            command.Parameters.AddWithValue("@Status", order.Status);
                            command.Parameters.AddWithValue("@UserId", order.UserId);
                            command.Parameters.AddWithValue("@TotalCost", order.TotalAmount);
                            command.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
                            command.Parameters.AddWithValue("@DeliveryTime", (object)order.DeliveryTime ?? DBNull.Value);

                            object result = await command.ExecuteScalarAsync();
                            orderId = Convert.ToInt32(result);
                        }

                        if (order.OrderDetails != null && order.OrderDetails.Count > 0)
                        {
                            foreach (var detail in order.OrderDetails)
                            {
                                if (detail.DishId.HasValue)
                                {
                                    using (NpgsqlCommand updateStockCommand = new NpgsqlCommand(@"
                                UPDATE Dishes
                                SET TotalQuantity = TotalQuantity - (PortionQuantity * @Quantity)
                                WHERE DishId = @DishId", connection, transaction))
                                    {
                                        updateStockCommand.Parameters.AddWithValue("@DishId", detail.DishId.Value);
                                        updateStockCommand.Parameters.AddWithValue("@Quantity", detail.Quantity);
                                        await updateStockCommand.ExecuteNonQueryAsync();
                                    }
                                }
                            }
                        }

                        transaction.Commit();
                        return orderId;
                    }
                    catch (Npgsql.PostgresException pgEx)
                    {
                        transaction.Rollback();

                        System.Diagnostics.Debug.WriteLine($"PostgreSQL error: {pgEx.MessageText}");

                        throw;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();

                        System.Diagnostics.Debug.WriteLine($"General error: {ex.Message}");

                        throw;
                    }
                }
            }
        }

        #endregion
    }
}