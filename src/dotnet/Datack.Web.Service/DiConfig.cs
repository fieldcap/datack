using Datack.Web.Service.Services;
using Datack.Web.Service.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace Datack.Web.Service
{
    public class DiConfig
    {
        public static void Config(IServiceCollection services)
        {
            services.AddDataProtection().SetApplicationName("Datack.Web");

            services.AddScoped<Authentication>();
            services.AddScoped<Emails>();
            services.AddScoped<JobRunner>();
            services.AddScoped<JobRuns>();
            services.AddScoped<JobRunTaskLogs>();
            services.AddScoped<JobRunTasks>();
            services.AddScoped<Jobs>();
            services.AddScoped<JobTasks>();
            services.AddScoped<RemoteService>();
            services.AddScoped<Agents>();
            services.AddScoped<Settings>();

            services.AddScoped<CreateBackupTask>();

            services.AddHostedService<StartupHostedService>();
            services.AddHostedService<SchedulerHostedService>();
        }
    }
}
