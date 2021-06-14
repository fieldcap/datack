using Microsoft.Extensions.DependencyInjection;
using Datack.Service.Services;

namespace Datack.Service
{
    public static class DiConfig
    {
        public static void Config(IServiceCollection services)
        {
            services.AddScoped<Authentication>();
            services.AddScoped<Jobs>();
            services.AddScoped<RemoteService>();
            services.AddScoped<Settings>();
            services.AddScoped<Servers>();
        }
    }
}