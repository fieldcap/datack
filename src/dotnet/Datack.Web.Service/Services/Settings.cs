using Datack.Common.Models.Data;
using Datack.Web.Data.Repositories;
using Microsoft.AspNetCore.DataProtection;
using Serilog.Core;

namespace Datack.Web.Service.Services;

public class Settings(SettingRepository settingRepository, IDataProtectionProvider provider)
{
    public static readonly LoggingLevelSwitch LoggingLevelSwitch = new();

    private readonly IDataProtector _protector = provider.CreateProtector("Datack.Web.Service.Settings");

    public async Task<IList<Setting>> GetAll(CancellationToken cancellationToken)
    {
        var settings = await settingRepository.GetAll(cancellationToken);

        foreach (var setting in settings)
        {
            if (setting.Secure)
            {
                setting.Value = "******";
            }
        }

        return settings;
    }

    public async Task Update(IList<Setting> settings, CancellationToken cancellationToken)
    {
        var allSettings = await GetAll(cancellationToken);

        foreach (var dbSetting in allSettings)
        {
            var updatedSetting = settings.FirstOrDefault(m => m.SettingId == dbSetting.SettingId);

            if (updatedSetting == null)
            {
                continue;
            }

            if (dbSetting.Secure)
            {
                if (updatedSetting.Value != null && updatedSetting.Value != "******")
                {
                    dbSetting.Value = _protector.Protect(updatedSetting.Value);
                    await settingRepository.Update(dbSetting, cancellationToken);
                }
            }
            else
            {
                dbSetting.Value = updatedSetting.Value;
                await settingRepository.Update(dbSetting, cancellationToken);
            }
        }
    }

    public async Task<T?> Get<T>(String key, CancellationToken cancellationToken)
    {
        var setting = await settingRepository.Get(key, cancellationToken);

        if (setting == null)
        {
            return default;
        }

        var value = setting.Value;

        if (String.IsNullOrWhiteSpace(value))
        {
            return default;
        }

        if (setting.Secure)
        {
            value = _protector.Unprotect(value);
        }

        return (T?)Convert.ChangeType(value, typeof(T?));
    }
}