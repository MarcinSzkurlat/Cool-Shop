using Codecool.CodecoolShop.Data;
using Codecool.CodecoolShop.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Codecool.CodecoolShop
{
	public static class ServiceCollectionExtensions
	{

		public static IServiceCollection AddSeeds(this IServiceCollection services)
		{
			services.AddScoped<CodeCoolShopSeed>();

			return services;
		}
		public static IServiceCollection AddServices(this IServiceCollection services)
		{
			AddShop(services);

			return services;
		}

		private static void AddShop(IServiceCollection services)
		{
			services.AddScoped<ProductService>();
			services.AddScoped<CartService>();
			services.AddScoped<SupplierService>();
			services.AddScoped<AddressService>();
		}
    }
}
