using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Datack.Data.Models.Data;

namespace Datack.Data.Data
{
    public class SettingData 
    {
        private readonly DataContext _dataContext;

        public SettingData(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<IList<Setting>> GetAll()
        {
            return await _dataContext.Settings.AsNoTracking().ToListAsync();
        }

        public async Task Update(IList<Setting> settings)
        {
            var dbSettings = await _dataContext.Settings.ToListAsync();

            foreach (var dbSetting in dbSettings)
            {
                var setting = settings.FirstOrDefault(m => m.SettingId == dbSetting.SettingId);

                if (setting != null)
                {
                    dbSetting.Value = setting.Value;
                }
            }

            await _dataContext.SaveChangesAsync();
        }

        public async Task<Setting> Get(String key)
        {
            return await _dataContext.Settings.AsNoTracking().FirstOrDefaultAsync(m => m.SettingId == key);
        }
    }
}
