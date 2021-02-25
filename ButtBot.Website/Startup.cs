using System.Net.Http;
using ButtBot.Library.Extentions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RMQCommandService.Extentions;
using Serilog;

namespace ButtBot.Website
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            
            services.DiscoverAndMakeDiServicesAvailable();
            services.AddSingleton<HttpClient>();

            services.AddRMQCommandService(options =>
            {
                options.Host = Configuration["RabbbitMq:Host"];
                options.User = Configuration["RabbbitMq:User"];
                options.Password = Configuration["RabbbitMq:Password"];
                options.DefaultBusService = "CORE";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSerilogRequestLogging();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStatusCodePagesWithReExecute("/StatusCode");
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{action=Index}",
                    defaults: new {controller = "Home"});
            });
        }
    }
}
