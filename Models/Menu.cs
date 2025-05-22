using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenuApp.Models
{
    public class Menu
    {
        public int MenuId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; } = true;
        public Category Category { get; set; } = new Category();
        public ObservableCollection<MenuDish> MenuDishes { get; set; } = new ObservableCollection<MenuDish>();
    }
}
