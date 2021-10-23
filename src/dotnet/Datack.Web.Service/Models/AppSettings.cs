﻿using System;

namespace Datack.Web.Service.Models
{
    public class AppSettings
    {
        public AppSettingsConnectionStrings ConnectionStrings { get; set; }

        public AppSettingsLogging Logging { get; set; }
        public String HostUrl { get; set; }
    }

    public class AppSettingsConnectionStrings
    {
        public String Datack { get; set; }
    }

    public class AppSettingsLogging
    {
        public AppSettingsLoggingFile File { get; set; }
    }

    public class AppSettingsLoggingFile
    {
        public String Path { get; set; }
        public Int64 FileSizeLimitBytes { get; set; }
        public Int32 MaxRollingFiles { get; set; }
    }
}
