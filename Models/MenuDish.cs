﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenuApp.Models
{
    public class MenuDish
    {
        public int Id { get; set; } 
        public int MenuId { get; set; }
        public int DishId { get; set; }
        public int Quantity { get; set; }
        public Dish Dish { get; set; } = new Dish();
    }
}
