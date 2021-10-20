using Datack.Web.Service.Services;
using Datack.Web.Service.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Datack.Web.Service
{
    public class DiConfig
    {
        public static void Config(IServiceCollection services)
        {
            services.AddScoped<Authentication>();
            services.AddScoped<Emails>();
            services.AddScoped<JobRunner>();
            services.AddScoped<JobRuns>();
            services.AddScoped<JobRunTaskLogs>();
            services.AddScoped<JobRunTasks>();
            services.AddScoped<Jobs>();
            services.AddScoped<JobTasks>();
            services.AddScoped<RemoteService>();
            services.AddScoped<Servers>();
            services.AddScoped<Settings>();

            services.AddScoped<CreateBackupTask>();

            services.AddHostedService<StartupHostedService>();
            services.AddHostedService<SchedulerHostedService>();
        }
    }
}
