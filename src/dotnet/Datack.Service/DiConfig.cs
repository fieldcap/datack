using Microsoft.Extensions.DependencyInjection;
using Datack.Service.Services;

namespace Datack.Service
{
    public static class DiConfig
    {
        public static void Config(IServiceCollection services)
        {
            services.AddScoped<IAuthentication, Authentication>();
            services.AddScoped<IRemoteService, RemoteService>();
            services.AddScoped<ISettings, Settings>();
        }
    }
}