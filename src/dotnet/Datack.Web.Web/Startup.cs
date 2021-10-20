using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Datack.Web.Data;
using Datack.Web.Service.Hubs;
using Datack.Web.Service.Middleware;
using Datack.Web.Service.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Datack.Web.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var appSettings = new AppSettings();
            Configuration.Bind(appSettings);

            services.AddSingleton(appSettings);

            services.AddDbContext<DataContext>(options => options.UseSqlServer(appSettings.ConnectionStrings.Datack));

            services.AddControllers()
                    .AddJsonOptions(opts =>
                    {
                        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                        opts.JsonSerializerOptions.Converters.Add(new JsonProtectedConverter());
                    });

            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "wwwroot"; });

            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            });
            
            services.AddHttpContextAccessor();

            services.AddCors(options =>
            {
                options.AddPolicy("Dev",
                                  builder =>
                                  {
                                      builder.AllowAnyHeader()
                                             .AllowAnyMethod()
                                             .AllowAnyOrigin();
                                  });
            });

            services.AddHttpClient();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options => { options.SlidingExpiration = true; });

            services.AddAuthorization();

            services.AddIdentity<IdentityUser, IdentityRole>(options =>
                    {
                        options.User.RequireUniqueEmail = false;
                        options.Password.RequiredLength = 10;
                        options.Password.RequireUppercase = false;
                        options.Password.RequireLowercase = false;
                        options.Password.RequireNonAlphanumeric = false;
                        options.Password.RequiredUniqueChars = 5;
                    })
                    .AddEntityFrameworkStores<DataContext>()
                    .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Cookie.Name = "SID";
            });

            // ReSharper disable RedundantNameQualifier
            Datack.Web.Data.DiConfig.Config(services);
            Datack.Web.Service.DiConfig.Config(services);
            // ReSharper restore RedundantNameQualifier
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseCors("Dev");
            }

            app.ConfigureExceptionHandler();

            app.Use(async (context, next) =>
            {
                await next.Invoke();

                if (context.Response.StatusCode != 200)
                {
                    logger.LogWarning($"{context.Response.StatusCode}: {context.Request.Path.Value}");
                }
            });

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<DatackHub>("/hub");
            });

            app.MapWhen(x => x.Request.Path.Value != null && !x.Request.Path.Value.StartsWith("/api"), builder =>
            {
                builder.UseSpaStaticFiles();
                builder.UseSpa(spa =>
                {
                    spa.Options.SourcePath = "wwwroot";
                    spa.Options.DefaultPage = "/index.html";
                });
            });

            var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
            logger.LogInformation("Listening on the following addresses: " + String.Join(", ", serverAddressesFeature.Addresses));
        }
    }
}