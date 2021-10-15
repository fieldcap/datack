using System;
using System.Text.Json.Serialization;

namespace Datack.Common.Models.Internal
{
    public class JobTaskSettings
    {
        [JsonPropertyName("createBackup")]
        public JobTaskCreateDatabaseSettings CreateBackup { get;set; }
        public JobTaskCompressSettings Compress { get;set; }
    }

    public class JobTaskCreateDatabaseSettings
    {
        [JsonPropertyName("fileName")]
        public String FileName { get;set; }

        [JsonPropertyName("backupDefaultExclude")]
        public Boolean BackupDefaultExclude{ get;set; }

        [JsonPropertyName("backupExcludeSystemDatabases")]
        public Boolean BackupExcludeSystemDatabases{ get;set; }

        [JsonPropertyName("backupIncludeRegex")]
        public String BackupIncludeRegex{ get;set; }
        
        [JsonPropertyName("backupExcludeRegex")]
        public String BackupExcludeRegex{ get;set; }
        
        [JsonPropertyName("backupIncludeManual")]
        public String BackupIncludeManual{ get;set; }

        [JsonPropertyName("backupExcludeManual")]
        public String BackupExcludeManual{ get;set; }
    }

    public class JobTaskCompressSettings
    {
        [JsonPropertyName("fileName")]
        public String FileName { get;set; }

        [JsonPropertyName("archiveType")]
        public String ArchiveType { get; set; }

        [JsonPropertyName("compressionLevel")]
        public String CompressionLevel { get; set; }
        
        [JsonPropertyName("multithreadMode")]
        public String MultithreadMode { get; set; }

        [JsonPropertyName("password")]
        public String Password { get; set; }
    }
}
