using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using Codecool.CodecoolShop.Data;
using Codecool.CodecoolShop.Migrations;
using Codecool.CodecoolShop.Models;
using Microsoft.AspNetCore.Identity;

namespace Codecool.CodecoolShop.Services;

public class CartService
{
    private readonly CodeCoolShopDBContext _dbContext;

    public CartService(CodeCoolShopDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void SaveCart(string userId, ShoppingCart cart)
    {
        var cartData = JsonSerializer.Serialize(cart);
        var record = new DatabaseCart();
        record.UserId = userId;
        record.ShoppingCartData = cartData;
        if (_dbContext.Carts.Any(c => c.UserId == userId))
        {
            _dbContext.Carts.Update(record);
        }
        else {
            _dbContext.Carts.Add(record);
        }
        _dbContext.SaveChanges();
    }

    public ShoppingCart? GetCart(string userId)
    {
        var databaseCart = _dbContext.Carts.FirstOrDefault(c => c.UserId == userId);
        Debug.WriteLine(databaseCart);
        if (databaseCart == null) return null;
        var data = databaseCart.ShoppingCartData;
        var shoppingCart = JsonSerializer.Deserialize<ShoppingCart>(data);
        return shoppingCart;
    }

    public void DeleteCart(string userId)
    {
        var databaseCart = _dbContext.Carts.FirstOrDefault(c => c.UserId == userId);
        _dbContext.Carts.Remove(databaseCart);
        _dbContext.SaveChanges();
    }
}