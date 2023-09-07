using Codecool.CodecoolShop.Models.UserData;
using Codecool.CodecoolShop.Models.Products;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Supplier = Codecool.CodecoolShop.Models.Products.Supplier;
using Codecool.CodecoolShop.Models.Cart;

namespace Codecool.CodecoolShop.Data;

public class CodeCoolShopDBContext : IdentityDbContext
{
    public CodeCoolShopDBContext(DbContextOptions<CodeCoolShopDBContext> options) : base(options)
    {

    }

    public virtual DbSet<Product> Products { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<BillingAddressModel> BillingAddressModels { get; set; }
    public DbSet<ShippingAddressModel> ShippingAddressModels { get; set; }
    public DbSet<DatabaseCart> Carts { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Product>().HasOne(x => x.Supplier);
    }
}