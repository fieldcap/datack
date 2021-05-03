using Microsoft.Extensions.DependencyInjection;
using Datack.Data.Data;

namespace Datack.Data
{
    public static class DiConfig
    {
        public static void Config(IServiceCollection services)
        {
            services.AddScoped<ISettingData, SettingData>();
            services.AddScoped<IUserData, UserData>();
        }
    }
}
