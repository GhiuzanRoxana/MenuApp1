using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MenuApp.Models
{
    public class CartItem : INotifyPropertyChanged
    {
        private int _itemId;
        private bool _isDish;
        private int _quantity = 1;
        private string _name = string.Empty;
        private decimal _price;

        public int ItemId
        {
            get => _itemId;
            set
            {
                if (_itemId != value)
                {
                    _itemId = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsDish
        {
            get => _isDish;
            set
            {
                if (_isDish != value)
                {
                    _isDish = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalPrice)); 
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value ?? string.Empty;
                    OnPropertyChanged();
                }
            }
        }

        public decimal Price
        {
            get => _price;
            set
            {
                if (_price != value)
                {
                    _price = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalPrice)); 
                }
            }
        }

        public decimal TotalPrice => Price * Quantity;

        public CartItem()
        {
            Name = string.Empty;
            Quantity = 1;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"{Name} x{Quantity} ({Price:C2} per bucată)";
        }
    }
}