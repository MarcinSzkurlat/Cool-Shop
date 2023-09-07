using Codecool.CodecoolShop.Models.Payment;
using Codecool.CodecoolShop.Models.UserData;
using Codecool.CodecoolShop.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Codecool.CodecoolShop.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Codecool.CodecoolShop.Models.Order;
using Codecool.CodecoolShop.Models.Products.DTO;
using Codecool.CodecoolShop.Models.Cart;

namespace Codecool.CodecoolShop.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ProductService _productService;
        private readonly IMapper _mapper;
        private readonly ILogger<CartController> _cartLogger;
        private readonly ShoppingCartService _shoppingCartLogic;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AddressService _addressService;

        public CartController(ProductService productService, IMapper mapper, ILogger<CartController> cartLogger,UserManager<IdentityUser> userManager, AddressService addressService)
        {
            _productService = productService;
            _mapper = mapper;
            _cartLogger = cartLogger;
            _shoppingCartLogic = new ShoppingCartService();
            _userManager = userManager;
            _addressService = addressService;
        }

        public IActionResult ViewCart()
        {
            var cart = _shoppingCartLogic.GetCart(HttpContext);
            var productIds = cart.Items.Keys.ToList();
            var products = productIds.Select(productId => _productService.GetProductById(productId)).ToList();

            var model = new CartViewModel
            {
                Cart = cart,
                Products = products,
                IsLoggedIn = HttpContext.User.Identity.IsAuthenticated,
            };

            return View(model);
        }

        public IActionResult AddToCart(int productId)
        {
            var cart = _shoppingCartLogic.GetCart(HttpContext);
            cart.Items.TryGetValue(productId, out var currentCount);
            cart.Items[productId] = currentCount + 1;
            _shoppingCartLogic.SaveCart(cart, HttpContext);
            return RedirectToAction("ViewCart");
        }

        public async Task<IActionResult> Checkout()
        {
            var cart = JsonSerializer.Deserialize<ShoppingCart>(HttpContext.Session.Get("Cart"));

            if (cart.Items.Count == 0) return StatusCode(403);
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var billingAddress = _addressService.FindBillingAddress(user.Id);
            var shippingAddress = _addressService.FindShippingAddress(user.Id);
            var newUser = new UserDataModel() { BillingAddress = billingAddress, ShippingAddress = shippingAddress, Email = user.Email, PhoneNumber = user.PhoneNumber, UserId = user.Id };
          
            if (HttpContext.Session.Get("UserData") == null) return View(newUser);
            
            var userData = JsonSerializer.Deserialize<UserDataModel>(HttpContext.Session.Get("UserData"));
            userData.ShippingAddress = shippingAddress;
            userData.BillingAddress = billingAddress;
            userData.UserId = user.Id;

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

            _cartLogger.LogInformation("{@order}", newOrder);

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
            _cartLogger.LogInformation("{@order}", newOrder);

            HttpContext.Session.SetString("OrderModel", JsonSerializer.Serialize(newOrder));


            return RedirectToAction("OrderConfirmation");
        }

        public async Task<IActionResult> OrderConfirmation()
        {
            if (HttpContext.Session.Get("UserData") == null
                || HttpContext.Session.Get("Payment") == null) return StatusCode(403);

            var user = await _userManager.GetUserAsync(HttpContext.User);
            var cart = JsonSerializer.Deserialize<ShoppingCart>(HttpContext.Session.Get("Cart"));

            var newOrder = JsonSerializer.Deserialize<OrderModel>(HttpContext.Session.GetString("OrderModel"));
            newOrder.OrderStatus = OrderStatus.Success;
            _cartLogger.LogInformation("{@ORDER}", newOrder);

            var order = new OrderModel()
            {
                Products = _productService.GetProductsCartByShoppingCart(cart),
                Payment = JsonSerializer.Deserialize<PaymentModel>(HttpContext.Session.Get("Payment")),
                UserData = JsonSerializer.Deserialize<UserDataModel>(HttpContext.Session.Get("UserData"))
            };

            order.UserData.UserId = user.Id;

            if (Request.Method == "POST")
            {
                HttpContext.Session.Clear();

                var productsDto = _mapper.Map<List<ProductDto>>(order.Products.Products);
                productsDto.ForEach(x => x.Subtotal = x.PricePerUnit * x.Quantity);

                var jsonOrder = new OrderToFileModel()
                {
                    OrderId = order.OrderId,
                    OrderDateTime = order.OrderDateTime,
                    OrderStatus = order.OrderStatus,
                    Payment = order.Payment,
                    UserData = order.UserData,
                    Products = productsDto
                };

                string filePath =
                    $"{AppDomain.CurrentDomain.BaseDirectory}\\orders\\{cart.Id}_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.json";

                SaveToFile.ToJson(jsonOrder, filePath);

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
