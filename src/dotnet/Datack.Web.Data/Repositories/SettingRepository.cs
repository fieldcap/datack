using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Datack.Web.Data.Repositories
{
    public class SettingRepository
    {
        private readonly DataContext _dataContext;

        public SettingRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<IList<Setting>> GetAll(CancellationToken cancellationToken)
        {
            return await _dataContext.Settings.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task Update(Setting setting, CancellationToken cancellationToken)
        {
            var dbSetting = await _dataContext.Settings.FirstOrDefaultAsync(m => m.SettingId == setting.SettingId, cancellationToken);

            dbSetting.Value = setting.Value;

            await _dataContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<Setting> Get(String key, CancellationToken cancellationToken)
        {
            return await _dataContext.Settings.AsNoTracking().FirstOrDefaultAsync(m => m.SettingId == key, cancellationToken);
        }
    }
}
