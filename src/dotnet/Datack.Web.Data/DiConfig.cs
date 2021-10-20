using Datack.Web.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Datack.Web.Data
{
    public class DiConfig
    {
        public static void Config(IServiceCollection services)
        {
            services.AddDataProtection();

            services.AddScoped<JobRepository>();
            services.AddScoped<JobRunRepository>();
            services.AddScoped<JobRunTaskLogRepository>();
            services.AddScoped<JobRunTaskRepository>();
            services.AddScoped<JobTaskRepository>();
            services.AddScoped<ServerRepository>();
            services.AddScoped<SettingRepository>();
            services.AddScoped<UserRepository>();
        }
    }
}
