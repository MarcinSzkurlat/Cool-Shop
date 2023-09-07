namespace Codecool.CodecoolShop.Models.Cart;

public class DatabaseCart
{
    [System.ComponentModel.DataAnnotations.Key]
    public string UserId { get; set; }
    public string ShoppingCartData { get; set; }
}