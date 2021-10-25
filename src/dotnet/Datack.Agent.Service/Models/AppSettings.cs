using System;

namespace Datack.Agent.Models
{
    public class AppSettings
    {
        public String Token { get; set; }
        public String ServerUrl { get; set; }

        public AppSettingsLogging Logging { get; set; }
        public AppSettingsDatabase Database { get; set; }
    }

    public class AppSettingsLogging
    {
        public AppSettingsLoggingLogLevel LogLevel { get; set; }
        public AppSettingsLoggingFile File { get; set; }
    }

    public class AppSettingsLoggingLogLevel
    {
        public String Default { get; set; }
    }
    
    public class AppSettingsLoggingFile
    {
        public String Path { get; set; }
        public Int64 FileSizeLimitBytes { get; set; }
        public Int32 MaxRollingFiles { get; set; }
    }

    public class AppSettingsDatabase
    {
        public String Path { get; set; }
    }
}
