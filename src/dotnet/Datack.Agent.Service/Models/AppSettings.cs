namespace Datack.Agent.Models;

public class AppSettings
{
    public String Token { get; set; } = default!;
    public String ServerUrl { get; set; } = default!;

    public AppSettingsLogging? Logging { get; set; }
}

public class AppSettingsLogging
{
    public AppSettingsLoggingLogLevel? LogLevel { get; set; }
    public AppSettingsLoggingFile? File { get; set; }
}

public class AppSettingsLoggingLogLevel
{
    public String? Default { get; set; }
}
    
public class AppSettingsLoggingFile
{
    public String? Path { get; set; }
    public Int64 FileSizeLimitBytes { get; set; }
    public Int32 MaxRollingFiles { get; set; }
}