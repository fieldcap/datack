using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Datack.Data.Data;
using Datack.Data.Models.Data;

namespace Datack.Service.Services
{
    public interface ISettings
    {
        Task<IList<Setting>> GetAll();
        Task Update(IList<Setting> settings);
        Task<String> GetString(String key);
        Task<Int32> GetNumber(String key);
    }

    public class Settings : ISettings
    {
        private readonly ISettingData _settingData;

        public Settings(ISettingData settingData)
        {
            _settingData = settingData;
        }

        public async Task<IList<Setting>> GetAll()
        {
            return await _settingData.GetAll();
        }

        public async Task Update(IList<Setting> settings)
        {
            await _settingData.Update(settings);
        }

        public async Task<String> GetString(String key)
        {
            var setting = await _settingData.Get(key);

            if (setting == null)
            {
                throw new Exception($"Setting with key {key} not found");
            }

            return setting.Value;
        }

        public async Task<Int32> GetNumber(String key)
        {
            var setting = await _settingData.Get(key);

            if (setting == null)
            {
                throw new Exception($"Setting with key {key} not found");
            }

            return Int32.Parse(setting.Value);
        }
    }
}
