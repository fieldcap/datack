using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Datack.Data.Data;

namespace Datack.Service.Services
{
    public class Settings
    {
        private readonly SettingData _settingData;

        public Settings(SettingData settingData)
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
