using System;

namespace Datack.Agent.Models
{
    public class AppSettings
    {
        public String Token { get; set; }

        public AppSettingsLogging Logging { get; set; }
        public AppSettingsDatabase Database { get; set; }
    }

    public class AppSettingsLogging
    {
        public AppSettingsLoggingFile File { get; set; }
    }
    
    public class AppSettingsLoggingFile
    {
        public String Path { get; set; }
        public String Append { get; set; }
        public Int64 FileSizeLimitBytes { get; set; }
        public Int32 MaxRollingFiles { get; set; }
    }

    public class AppSettingsDatabase
    {
        public String Path { get; set; }
    }
}
