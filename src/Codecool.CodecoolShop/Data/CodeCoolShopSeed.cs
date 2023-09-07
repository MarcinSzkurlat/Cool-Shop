using System.Linq;
using Bogus;
using Codecool.CodecoolShop.Models.Products;

namespace Codecool.CodecoolShop.Data;

public class CodeCoolShopSeed
{
    private readonly CodeCoolShopDBContext _codeCoolShopDbContext;

    public CodeCoolShopSeed(CodeCoolShopDBContext codeCoolShopDbContext)
    {
        _codeCoolShopDbContext = codeCoolShopDbContext;
    }

    public void Seed()
    {
        _codeCoolShopDbContext.Database.EnsureCreated();

        if (!_codeCoolShopDbContext.Products.Any())
        {
            var supplierFaker = new Faker<Supplier>()
                .RuleFor(p => p.Name, f => f.Company.CompanyName())
                .RuleFor(p => p.Description, f => f.Lorem.Sentence(5));

            var supplierList = supplierFaker.Generate(10);

            var productFaker = new Faker<Product>()
                .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                .RuleFor(p => p.Currency, "USD")
                .RuleFor(p => p.DefaultPrice, f => f.Random.Decimal(50, 1000))
                .RuleFor(p => p.ProductCategory, f => (ProductCategory)f.Random.Int(0, 2))
                .RuleFor(p => p.Description, f => f.Lorem.Sentence(5))
                .RuleFor(p => p.Supplier, f => supplierList[f.Random.Int(0, supplierList.Count - 1)])
                .RuleFor(p => p.Image, f => f.Random.Int(1, 20).ToString());


            var products = productFaker.Generate(100);
            _codeCoolShopDbContext.AddRange(products);
            _codeCoolShopDbContext.SaveChanges();
        }
    }
}