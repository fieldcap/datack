using Datack.Web.Data.Models;
using Datack.Web.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Datack.Web.Data;

public class DiConfig
{
    public static void Config(IServiceCollection services, AppSettings appSettings)
    {
        var connectionString = $"Data Source={appSettings.Database.Path}";
        services.AddDbContext<DataContext>(options => options.UseSqlite(connectionString));

        services.AddScoped<JobRepository>();
        services.AddScoped<JobRunRepository>();
        services.AddScoped<JobRunTaskLogRepository>();
        services.AddScoped<JobRunTaskRepository>();
        services.AddScoped<JobTaskRepository>();
        services.AddScoped<AgentRepository>();
        services.AddScoped<SettingRepository>();
        services.AddScoped<UserRepository>();
    }
}