using System;
using System.IO;
using Codecool.CodecoolShop.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Json;

namespace Codecool.CodecoolShop
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var id = new Random().Next(1000);
            var date = DateTime.UtcNow;
            FilePath.Path = Path.Combine(Environment.CurrentDirectory, "Data", "Log", $"{date.ToString()+ id}.json");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Console(new JsonFormatter())
                .WriteTo.File(new JsonFormatter(), FilePath.Path)
                .Filter.ByIncludingOnly(evt => evt.Properties.ContainsKey("SourceContext") && evt.Properties["SourceContext"].ToString().Contains("CartController"))
                .CreateLogger();

            try
            {
                Log.Information("Application Starting Up");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Cos nie dziala kierowniku");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
