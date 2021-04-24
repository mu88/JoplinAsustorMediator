using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace JoplinAsustorMediator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((_, config) => { config.AddEnvironmentVariables(); })
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>().UseUrls("http://localhost:5200/", "https://localhost:5201/"); });
        }
    }
}