﻿using Codecool.CodecoolShop.Logic;
using Codecool.CodecoolShop.Models;
using Codecool.CodecoolShop.Models.DTO;
using Codecool.CodecoolShop.Models.Payment;
using Codecool.CodecoolShop.Models.UserData;
using Codecool.CodecoolShop.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog.Formatting.Json;
using Serilog;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using AutoMapper;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Codecool.CodecoolShop.Controllers
{
    public class CartController : Controller
    {

        private ILogger<ProductController> _logger;
        private readonly ProductService _productService;
        private readonly IMapper _mapper;
        private ILogger? Logger;

        
        public CartController(ILogger<ProductController> logger, ProductService productService, IMapper mapper)
        {
            _logger = logger;
            _productService = productService;
            _mapper = mapper;

        }

        public IActionResult ViewCart()
        {
            var cart = GetCart();
            var productIds = cart.Items.Keys.ToList();
            var products = productIds.Select(productId => _productService.GetProduct(productId)).ToList();

            var model = new CartViewModel
            {
                Cart = cart,
                Products = products
            };

            return View(model);
        }

        public IActionResult AddToCart(int productId)
        {
            var cart = GetCart();
            cart.Items.TryGetValue(productId, out var currentCount);
            cart.Items[productId] = currentCount + 1;
            SaveCart(cart);
            return RedirectToAction("ViewCart");
        }

        public ShoppingCart GetCart()
        {
            ShoppingCart cart;
            if (HttpContext.Session.Get("Cart") != null)
            {
                Debug.WriteLine("Found existing cart");
                cart = JsonSerializer.Deserialize<ShoppingCart>(HttpContext.Session.Get("Cart"));
            }
            else
            {
                Debug.WriteLine("Created new cart");
                cart = new ShoppingCart();
                SaveCart(cart);
            }

            return cart;
        }

        public void SaveCart(ShoppingCart cart)
        {
            Debug.WriteLine("Saved cart");
            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
        }

        public IActionResult Checkout()
        {
            var cart = JsonSerializer.Deserialize<ShoppingCart>(HttpContext.Session.Get("Cart"));

            if (cart.Items.Count == 0) return StatusCode(403);
            if (HttpContext.Session.Get("UserData") == null) return View();

            var userData = JsonSerializer.Deserialize<UserDataModel>(HttpContext.Session.Get("UserData"));

            return View(userData);
        }

        [HttpPost]
        public IActionResult Checkout(UserDataModel userData)
        {

            if (!ModelState.IsValid)
            {
                return View(userData);
            }

            var newOrder = new OrderModel
            {
                OrderStatus = OrderStatus.Received,
                UserData = userData
            };



            FilePath.Path = Path.Combine(Environment.CurrentDirectory, "Data", "Log", $"{newOrder.OrderId}.json");
            var log = new LoggerConfiguration()
                .WriteTo.File(new JsonFormatter(), FilePath.Path)
                .CreateLogger();

            log.Information("Order UserName: {@name} " +
                            "Shipping Address: {@Address}" +
                            "Order Status : {@orderstatus}", newOrder.UserData.Name, newOrder.UserData.ShippingAddress, newOrder.OrderStatus);


            HttpContext.Session.SetString("UserData", JsonSerializer.Serialize(userData));
            HttpContext.Session.SetString("OrderModel", JsonSerializer.Serialize(newOrder));


            return RedirectToAction("Payment");
        }

        public IActionResult Payment()
        {
            if (HttpContext.Session.Get("UserData") == null) return StatusCode(403);

            return View();
        }

        [HttpPost]
        public IActionResult Payment(PaymentModel payment)
        {
            if (!ModelState.IsValid)
            {
                return View(payment);
            }

     


            HttpContext.Session.SetString("Payment", JsonSerializer.Serialize(payment));

            var newOrder = JsonSerializer.Deserialize<OrderModel>(HttpContext.Session.GetString("OrderModel"));
            newOrder.OrderStatus = OrderStatus.MoneyReceived;
            Debug.Write(newOrder.OrderStatus);
            var log = new LoggerConfiguration()
                .WriteTo.File(new JsonFormatter(), FilePath.Path)
                .CreateLogger();

            log.Information("Order UserName: {@name} " +
                            "Shipping Address: {@Address}" +
                            "Order Status : {@orderstatus}", newOrder.UserData.Name, newOrder.UserData.ShippingAddress, newOrder.OrderStatus);

            log.Information("HELLLOOOOO");
            HttpContext.Session.SetString("OrderModel", JsonSerializer.Serialize(newOrder));


            return RedirectToAction("OrderConfirmation");
        }

        public IActionResult OrderConfirmation()
        {

            if (HttpContext.Session.Get("UserData") == null
                || HttpContext.Session.Get("Payment") == null) return StatusCode(403);


            var cart = JsonSerializer.Deserialize<ShoppingCart>(HttpContext.Session.Get("Cart"));

            var newOrder = JsonSerializer.Deserialize<OrderModel>(HttpContext.Session.GetString("OrderModel"));
            newOrder.OrderStatus = OrderStatus.Success;

            var log = new LoggerConfiguration()
                .WriteTo.File(new JsonFormatter(), FilePath.Path)
                .CreateLogger();

            log.Information("Order UserName: {@name} " +
                            "Shipping Address: {@Address}" +
                            "Order Status : {@orderstatus}", newOrder.UserData.Name, newOrder.UserData.ShippingAddress, newOrder.OrderStatus);



            log.Information("HELLLOOOOO");



            var order = new OrderModel()
            {
                Products = _productService.GetProductsCartByShoppingCart(cart),
                Payment = JsonSerializer.Deserialize<PaymentModel>(HttpContext.Session.Get("Payment")),
                UserData = JsonSerializer.Deserialize<UserDataModel>(HttpContext.Session.Get("UserData"))
            };


            if (Request.Method == "POST")
            {
                HttpContext.Session.Clear();

                var productsDto = _mapper.Map<List<ProductDto>>(order.Products.Products);
                productsDto.ForEach(x => x.Subtotal = x.PricePerUnit * x.Quantity);

                var jsonOrder = new OrderToFileModel()
                {
                    Payment = order.Payment,
                    UserData = order.UserData,
                    Products = productsDto
                };

                string filePath =
                    $"{AppDomain.CurrentDomain.BaseDirectory}\\orders\\{cart.Id}_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.json";

                //SaveToFile.ToJson(jsonOrder, filePath);

                //TODO send email to user about order

                return RedirectToAction("Index", "Product");
            }
            return View(order);
        }
    }

    public class FilePath
    {
        public static string Path { get; set; }

    }
}
