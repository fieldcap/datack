using System.Diagnostics;
using System.Text.Json.Serialization;
using Datack.Common.Helpers;
using Datack.Web.Data;
using Datack.Web.Data.Models;
using Datack.Web.Service.Hubs;
using Datack.Web.Service.Middleware;
using Datack.Web.Service.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting.WindowsServices;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Exceptions;

try
{
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        Args = args,
        ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default
    });

    // Bind the AppSettings from the appsettings.json files.
    builder.Configuration.AddJsonFile("appsettings.json", false, false);
    builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, false);

    // Bind AppSettings
    var appSettings = new AppSettings();
    builder.Configuration.Bind(appSettings);
    builder.Services.AddSingleton(appSettings);

    // Configure URLs
    if (appSettings.Port <= 0)
    {
        appSettings.Port = 6500;
    }

    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(appSettings.Port);
    });

    if (appSettings.Logging?.File?.Path != null)
    {
        builder.Host.UseSerilog((_, lc) => lc.Enrich.FromLogContext()
                                             .Enrich.WithExceptionDetails()
                                             .WriteTo.File(appSettings.Logging.File.Path,
                                                           rollOnFileSizeLimit: true,
                                                           fileSizeLimitBytes: appSettings.Logging.File.FileSizeLimitBytes,
                                                           retainedFileCountLimit: appSettings.Logging.File.MaxRollingFiles,
                                                           outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
                                             .WriteTo.Console()
                                             .MinimumLevel.ControlledBy(Settings.LoggingLevelSwitch)
                                             .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                                             .MinimumLevel.Override("System.Net.Http", LogEventLevel.Warning));
    }

    builder.Host.UseWindowsService();

    SelfLog.Enable(msg =>
    {
        Debug.Print(msg);
        Debugger.Break();
        Console.WriteLine(msg);

        if (!Directory.Exists(@"C:\Temp\Datack"))
        {
            Directory.CreateDirectory(@"C:\Temp\Datack");
        }
        File.WriteAllText($@"C:\Temp\Datack\{Guid.NewGuid()}.txt", msg);
    });

    Log.Information($"Starting host version {VersionHelper.GetVersion()}");

    builder.Services.AddControllers()
           .AddJsonOptions(opts =>
           {
               opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
               opts.JsonSerializerOptions.Converters.Add(new JsonProtectedConverter());
           });

    builder.Services
           .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
           .AddCookie(options =>
           {
               options.SlidingExpiration = true;
           });

    builder.Services.AddAuthorization();

    builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
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

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            return Task.CompletedTask;
        };

        options.Cookie.Name = "Datack";
    });

    builder.Services.Configure<HostOptions>(hostOptions =>
    {
        hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
    });

    // Configure development cors.
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Dev",
                          corsBuilder => corsBuilder.AllowAnyMethod()
                                                    .AllowAnyHeader()
                                                    .AllowCredentials());
    });

    // Configure misc services.
    builder.Services.AddResponseCaching();
    builder.Services.AddMemoryCache();
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddHttpClient();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddSession();

    builder.Services.AddSpaStaticFiles(spaBuilder =>
    {
        spaBuilder.RootPath = "wwwroot";
    });

    builder.Services.AddSignalR(hubOptions =>
    {
        hubOptions.EnableDetailedErrors = true;
        hubOptions.MaximumReceiveMessageSize = null;
        hubOptions.MaximumParallelInvocationsPerClient = 2;
    });

    // ReSharper disable RedundantNameQualifier
    Datack.Web.Data.DiConfig.Config(builder.Services, appSettings);
    Datack.Web.Service.DiConfig.Config(builder.Services);
    // ReSharper restore RedundantNameQualifier

    // Build the app
    var app = builder.Build();

    if (builder.Environment.IsDevelopment())
    {
        app.UseCors("Dev");
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseHsts();
        app.UseHttpsRedirection();
    }

    app.ConfigureExceptionHandler();

    app.Use(async (context, next) =>
    {
        await next.Invoke();

        if (context.Response.StatusCode != 200)
        {
            Log.Warning("{StatusCode}: {Value}", context.Response.StatusCode, context.Request.Path.Value);
        }
    });

    app.UseRouting();

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapHub<AgentHub>("/hubs/agent");
    app.MapHub<WebHub>("/hubs/web");
    app.MapControllers();

    app.UseWhen(x => !x.Request.Path.StartsWithSegments("/api"),
                routeBuilder =>
                {
                    routeBuilder.UseSpaStaticFiles();

                    routeBuilder.UseSpa(spa =>
                    {
                        spa.Options.SourcePath = "wwwroot";
                        spa.Options.DefaultPage = "/index.html";
                    });
                });

    // Run the app
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}