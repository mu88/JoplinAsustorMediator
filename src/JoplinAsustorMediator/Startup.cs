using System.Net.Http;
using AspNetCore.Proxy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JoplinAsustorMediator
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddProxies();

            services.Configure<AppSettings>(Configuration.GetSection($"{nameof(AppSettings)}"));

            if (bool.TryParse(Configuration[$"{nameof(AppSettings)}:{nameof(AppSettings.CustomTlsValidation)}"], out var customTlsValidation) && customTlsValidation)
            {
                var certThumbprint = Configuration[$"{nameof(AppSettings)}:{nameof(AppSettings.CertThumbprint)}"];
                services.AddHttpClient("CustomHttpClient")
                    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (_, cert, _, _) => cert.GetCertHashString() == certThumbprint
                    });
            }
            else
                services.AddHttpClient("CustomHttpClient");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UsePathBase("/joplin");

            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}