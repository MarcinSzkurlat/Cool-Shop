using System;
using Codecool.CodecoolShop.Data;
using Codecool.CodecoolShop.Mappings;
using Codecool.CodecoolShop.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;


namespace Codecool.CodecoolShop
{
	public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllersWithViews();
			services.AddDbContext<CodeCoolShopDBContext>(options =>
				options.UseSqlServer(Environment.GetEnvironmentVariable("ConnectionString")));
			services.AddDefaultIdentity<IdentityUser>()
				.AddEntityFrameworkStores<CodeCoolShopDBContext>();

			services.AddSeeds();
			services.AddServices();
			services.AddDistributedMemoryCache();
			services.AddSession();
			services.AddAutoMapper(typeof(ProductMappingProfile));
			services.AddScoped<OrderHistoryService>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Product/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSession();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Product}/{action=Index}/{id?}");
            });

            var scope = app.ApplicationServices.CreateScope();
            var seeder = scope.ServiceProvider.GetService<CodeCoolShopSeed>();
            seeder.Seed();

        }
    }
}
