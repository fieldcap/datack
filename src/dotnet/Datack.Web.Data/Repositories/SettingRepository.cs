using Datack.Common.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Datack.Web.Data.Repositories;

public class SettingRepository(DataContext dataContext)
{
    public async Task<IList<Setting>> GetAll(CancellationToken cancellationToken)
    {
        return await dataContext.Settings.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task Update(Setting setting, CancellationToken cancellationToken)
    {
        var dbSetting = await dataContext.Settings.FirstOrDefaultAsync(m => m.SettingId == setting.SettingId, cancellationToken);

        if (dbSetting != null)
        {

            dbSetting.Value = setting.Value;

            await dataContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<Setting?> Get(String key, CancellationToken cancellationToken)
    {
        return await dataContext.Settings.AsNoTracking().FirstOrDefaultAsync(m => m.SettingId == key, cancellationToken);
    }
}