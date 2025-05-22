using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenuApp.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int UserId { get; set; }
        public User User { get; set; } = new User();
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal TotalCost { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public ObservableCollection<OrderDetail> OrderDetails { get; set; } = new ObservableCollection<OrderDetail>();
        public ObservableCollection<OrderDetail> OrderDishes =>
        new ObservableCollection<OrderDetail>(OrderDetails.Where(d => d.DishId.HasValue));

        public ObservableCollection<OrderDetail> OrderMenus =>
            new ObservableCollection<OrderDetail>(OrderDetails.Where(m => m.MenuId.HasValue));

        public decimal DeliveryCost { get; set; } = 30.00m;
        public decimal Discount { get; set; } = 0.00m;


        public string TransportInfo => "Transport gratuit";
        public DateTime? EstimatedDeliveryTime { get; set; }
        public string DiscountInfo => "0%";


        public decimal Total
        {
            get
            {
                return TotalCost + DeliveryCost - Discount;
            }
        }

    }
}
