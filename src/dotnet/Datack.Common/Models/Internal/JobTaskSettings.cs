using System;
using System.Text.Json.Serialization;

namespace Datack.Common.Models.Internal
{
    public class JobTaskSettings
    {
        [JsonPropertyName("createBackup")]
        public JobTaskCreateDatabaseSettings CreateBackup { get; set; }

        [JsonPropertyName("compress")]
        public JobTaskCompressSettings Compress { get; set; }

        [JsonPropertyName("uploadS3")]
        public JobTaskUploadS3Settings UploadS3 { get; set; }
    }

    public class JobTaskCreateDatabaseSettings
    {
        [JsonPropertyName("fileName")]
        public String FileName { get; set; }

        [JsonPropertyName("backupType")]
        public String BackupType { get; set; }

        [JsonPropertyName("backupDefaultExclude")]
        public Boolean BackupDefaultExclude { get; set; }

        [JsonPropertyName("backupExcludeSystemDatabases")]
        public Boolean BackupExcludeSystemDatabases { get; set; }

        [JsonPropertyName("backupIncludeRegex")]
        public String BackupIncludeRegex { get; set; }

        [JsonPropertyName("backupExcludeRegex")]
        public String BackupExcludeRegex { get; set; }

        [JsonPropertyName("backupIncludeManual")]
        public String BackupIncludeManual { get; set; }

        [JsonPropertyName("backupExcludeManual")]
        public String BackupExcludeManual { get; set; }
    }

    public class JobTaskCompressSettings
    {
        [JsonPropertyName("fileName")]
        public String FileName { get; set; }

        [JsonPropertyName("archiveType")]
        public String ArchiveType { get; set; }

        [JsonPropertyName("compressionLevel")]
        public String CompressionLevel { get; set; }

        [JsonPropertyName("multithreadMode")]
        public String MultithreadMode { get; set; }

        [JsonPropertyName("password")]
        public String Password { get; set; }
    }

    public class JobTaskUploadS3Settings
    {
        [JsonPropertyName("fileName")]
        public String FileName { get; set; }

        [JsonPropertyName("region")]
        public String Region { get; set; }

        [JsonPropertyName("bucket")]
        public String Bucket { get; set; }

        [JsonPropertyName("accessKey")]
        public String AccessKey { get; set; }

        [JsonPropertyName("secret")]
        public String Secret { get; set; }
    }
}
