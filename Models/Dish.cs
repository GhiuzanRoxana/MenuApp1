using System;
using System.Collections.ObjectModel;

namespace MenuApp.Models
{
    public class Dish
    {
        public int DishId { get; set; }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => _name = value ?? string.Empty;
        }

        public decimal Price { get; set; }
        public decimal PortionQuantity { get; set; }
        public decimal TotalQuantity { get; set; }
        public int CategoryId { get; set; }
        public string? ImagePath { get; set; }
        public Category Category { get; set; } = new Category();
        public ObservableCollection<Allergen> Allergens { get; set; } = new ObservableCollection<Allergen>();

        public bool IsAvailable
        {
            get => TotalQuantity >= PortionQuantity;
            set { } 
        }

        public Dish()
        {
            Name = string.Empty;
            Category = new Category();
            Allergens = new ObservableCollection<Allergen>();
        }

        public override string ToString()
        {
            return $"{Name} ({Price:C2}) - {(IsAvailable ? "Disponibil" : "Indisponibil")}";
        }
    }
}