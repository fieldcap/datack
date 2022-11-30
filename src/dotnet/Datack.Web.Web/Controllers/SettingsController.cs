using Datack.Common.Models.Data;
using Datack.Web.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog.Events;

namespace Datack.Web.Web.Controllers;

[Authorize]
[Route("Api/Settings")]
public class SettingsController : Controller
{
    private readonly Settings _settings;
    private readonly Emails _emails;

    public SettingsController(Settings settings, Emails emails)
    {
        _settings = settings;
        _emails = emails;
    }

    [HttpGet]
    [Route("")]
    public async Task<ActionResult<IList<Setting>>> Get(CancellationToken cancellationToken)
    {
        var result = await _settings.GetAll(cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    [Route("")]
    public async Task<ActionResult> Update([FromBody] IList<Setting> settings, CancellationToken cancellationToken)
    {
        await _settings.Update(settings, cancellationToken);
            
        var logLevelSetting = await _settings.Get<String>("LogLevel", cancellationToken);

        if (!Enum.TryParse<LogEventLevel>(logLevelSetting, out var logLevel))
        {
            logLevel = LogEventLevel.Information;
        }

        Settings.LoggingLevelSwitch.MinimumLevel = logLevel;

        return Ok();
    }

    [HttpPost]
    [Route("TestEmail")]
    public async Task<ActionResult> TestEmail([FromBody] SettingsTestEmailRequest request, CancellationToken cancellationToken)
    {
        await _emails.SendTest(request.To, cancellationToken);

        return Ok();
    }
}

public class SettingsTestEmailRequest
{
    public required String To { get; set; }
}