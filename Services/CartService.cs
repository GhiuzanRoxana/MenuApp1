using MenuApp.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MenuApp.Services
{
    public class CartService
    {
        private static CartService? _instance;
        public static CartService Instance => _instance ??= new CartService();

        private CartService()
        {
            CartChanged = delegate { };
        }

        public ObservableCollection<CartItem> Items { get; } = new ObservableCollection<CartItem>();

        public decimal TotalPrice => Items.Sum(item => item.TotalPrice);
        public int ItemCount => Items.Sum(item => item.Quantity);

        public event EventHandler CartChanged;

        public void AddItem(Dish dish, int quantity = 1)
        {
            if (dish == null)
            {
                Debug.WriteLine("Încercare de a adăuga un dish null în coș!");
                return;
            }

            Debug.WriteLine($"Adăugare în coș: ID={dish.DishId}, Nume={dish.Name}, Preț={dish.Price:N2}, Cantitate={quantity}");

            var existingItem = Items.FirstOrDefault(i => i.IsDish && i.ItemId == dish.DishId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                Debug.WriteLine($"Produs existent actualizat: {existingItem.Name}, Cantitate nouă: {existingItem.Quantity}");
                CartChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                var newItem = new CartItem
                {
                    ItemId = dish.DishId,
                    IsDish = true,
                    Name = dish.Name ?? "Produs necunoscut",
                    Price = dish.Price,
                    Quantity = quantity
                };

                Items.Add(newItem);
                Debug.WriteLine($"Produs nou adăugat în coș: {newItem.Name}, Cantitate: {newItem.Quantity}");
                CartChanged?.Invoke(this, EventArgs.Empty);
            }

            Debug.WriteLine($"Stare coș actualizată: {Items.Count} produse, Total: {TotalPrice:N2} lei");
        }

        public void AddItem(Menu menu, int quantity = 1)
        {
            if (menu == null)
            {
                Debug.WriteLine("Încercare de a adăuga un menu null în coș!");
                return;
            }

            Debug.WriteLine($"Adăugare menu în coș: ID={menu.MenuId}, Nume={menu.Name}, Preț={menu.Price:N2}, Cantitate={quantity}");

            var existingItem = Items.FirstOrDefault(i => !i.IsDish && i.ItemId == menu.MenuId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                Debug.WriteLine($"Menu existent actualizat: {existingItem.Name}, Cantitate nouă: {existingItem.Quantity}");
                CartChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                var newItem = new CartItem
                {
                    ItemId = menu.MenuId,
                    IsDish = false,
                    Name = menu.Name ?? "Menu necunoscut",
                    Price = menu.Price,
                    Quantity = quantity
                };

                Items.Add(newItem);
                Debug.WriteLine($"Menu nou adăugat în coș: {newItem.Name}, Cantitate: {newItem.Quantity}");
                CartChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void RemoveItem(CartItem item)
        {
            if (item == null) return;

            Debug.WriteLine($"Eliminare produs din coș: {item.Name}, Cantitate: {item.Quantity}");
            Items.Remove(item);
            CartChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateItemQuantity(CartItem item, int newQuantity)
        {
            if (item == null) return;

            Debug.WriteLine($"Actualizare cantitate pentru {item.Name}: de la {item.Quantity} la {newQuantity}");

            if (newQuantity <= 0)
            {
                RemoveItem(item);
            }
            else
            {
                item.Quantity = newQuantity;
                CartChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void ClearCart()
        {
            Debug.WriteLine($"Golire coș: {Items.Count} produse eliminate");
            Items.Clear();
            CartChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool IsItemInCart(int itemId, bool isDish)
        {
            return Items.Any(i => i.ItemId == itemId && i.IsDish == isDish);
        }

        public int GetItemQuantity(int itemId, bool isDish)
        {
            var item = Items.FirstOrDefault(i => i.ItemId == itemId && i.IsDish == isDish);
            return item?.Quantity ?? 0;
        }

        public async Task<Order?> CreateOrderFromCartAsync(User user, DatabaseService databaseService)
        {
            if (user == null || Items.Count == 0)
                return null;

            Debug.WriteLine($"Creare comandă pentru utilizatorul {user.FirstName} {user.LastName}");

            Order order = new Order
            {
                OrderCode = GenerateOrderCode(),
                OrderDate = DateTime.Now,
                Status = "inregistrata",
                UserId = user.UserId,
                CustomerName = $"{user.FirstName} {user.LastName}",
                CustomerPhone = user.PhoneNumber,
                CustomerAddress = user.DeliveryAddress,
                TotalAmount = TotalPrice,
                OrderDetails = new ObservableCollection<OrderDetail>()
            };

            foreach (var item in Items)
            {
                Debug.WriteLine($"Adăugare în comandă: {item.Name}, Cantitate: {item.Quantity}, Preț: {item.Price:N2}");

                OrderDetail detail = new OrderDetail
                {
                    DishId = item.IsDish ? item.ItemId : (int?)null,
                    MenuId = !item.IsDish ? item.ItemId : (int?)null,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price,
                    Amount = item.TotalPrice
                };
                order.OrderDetails.Add(detail);
            }

            int orderId = await databaseService.CreateOrderAsync(order);
            if (orderId > 0)
            {
                Debug.WriteLine($"Comandă creată cu succes. ID: {orderId}, Cod: {order.OrderCode}");
                ClearCart();
                return order;
            }

            Debug.WriteLine("Eroare la crearea comenzii în baza de date");
            return null;
        }

        private string GenerateOrderCode()
        {
            return $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
        }
    }
}