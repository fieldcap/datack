namespace Datack.Web.Data.Models;

public class AppSettings
{
    public AppSettingsLogging? Logging { get; set; }
    public AppSettingsDatabase Database { get; set; } = new();

    public Int32 Port { get; set; }
}

public class AppSettingsLogging
{
    public AppSettingsLoggingFile? File { get; set; }
}

public class AppSettingsLoggingFile
{
    public String? Path { get; set; }
    public Int64 FileSizeLimitBytes { get; set; }
    public Int32 MaxRollingFiles { get; set; }
}

public class AppSettingsDatabase
{
    public String Path { get; set; } = default!;
}