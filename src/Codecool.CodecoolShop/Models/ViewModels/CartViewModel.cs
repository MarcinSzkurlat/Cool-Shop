﻿using System.Collections.Generic;
using Codecool.CodecoolShop.Models.Cart;
using Codecool.CodecoolShop.Models.Products;

namespace Codecool.CodecoolShop.Models.ViewModels
{
    public class CartViewModel
    {
        public ShoppingCart Cart { get; set; }
        public List<Product> Products { get; set; }
        public bool IsLoggedIn { get; set; }
    }
}