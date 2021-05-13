using ChatApp.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace ChatApp
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
            services.AddCors(options => options.AddPolicy("CorsPolicy",
                        builder =>
                        {
                            builder.AllowAnyMethod().AllowAnyHeader()
                                   .WithOrigins("http://localhost:62538")
                                   .AllowCredentials();
                        }));

            services.AddSignalR()
                .AddHubOptions<ChatHub>(options =>
                {
                    options.EnableDetailedErrors = true;
                })
                .AddAzureSignalR();
            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.RollingFile("ErrorLogs/log-{Date}.txt", retainedFileCountLimit: 7, shared: true,
                        restrictedToMinimumLevel: LogEventLevel.Warning)
                    .CreateLogger();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseCors("CorsPolicy");
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>("/chatHub");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
