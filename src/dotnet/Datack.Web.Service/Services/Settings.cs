using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Datack.Web.Data.Repositories;

namespace Datack.Web.Service.Services
{
    public class Settings
    {
        private readonly SettingRepository _settingRepository;

        public Settings(SettingRepository settingRepository)
        {
            _settingRepository = settingRepository;
        }

        public async Task<IList<Setting>> GetAll()
        {
            return await _settingRepository.GetAll();
        }

        public async Task Update(IList<Setting> settings)
        {
            await _settingRepository.Update(settings);
        }

        public async Task<Setting> Get(String key)
        {
            return await _settingRepository.Get(key);
        }
    }
}
