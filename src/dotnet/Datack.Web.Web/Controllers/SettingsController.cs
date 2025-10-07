using Datack.Common.Models.Data;
using Datack.Web.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog.Events;

namespace Datack.Web.Web.Controllers;

[Authorize]
[Route("Api/Settings")]
public class SettingsController(Settings settings, Emails emails) : Controller
{
    [HttpGet]
    [Route("")]
    public async Task<ActionResult<IList<Setting>>> Get(CancellationToken cancellationToken)
    {
        var result = await settings.GetAll(cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    [Route("")]
    public async Task<ActionResult> Update([FromBody] IList<Setting> settings1, CancellationToken cancellationToken)
    {
        await settings.Update(settings1, cancellationToken);
            
        var logLevelSetting = await settings.Get<String>("LogLevel", cancellationToken);

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
        await emails.SendTest(request.To, cancellationToken);

        return Ok();
    }
}

public class SettingsTestEmailRequest
{
    public required String To { get; set; }
}