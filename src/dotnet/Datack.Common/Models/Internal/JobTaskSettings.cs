using System.Text.Json.Serialization;
using Datack.Common.Helpers;

namespace Datack.Common.Models.Internal;

public class JobTaskSettings
{
    [JsonPropertyName("createBackup")]
    public JobTaskCreateDatabaseSettings? CreateBackup { get; set; }

    [JsonPropertyName("compress")]
    public JobTaskCompressSettings? Compress { get; set; }

    [JsonPropertyName("deleteFile")]
    public JobTaskDeleteFileSettings? DeleteFile { get; set; }

    [JsonPropertyName("deleteS3")]
    public JobTaskDeleteS3Settings? DeleteS3 { get; set; }

    [JsonPropertyName("downloadS3")]
    public JobTaskDownloadS3Settings? DownloadS3 { get; set; }
    
    [JsonPropertyName("downloadAzure")]
    public JobTaskDownloadAzureSettings? DownloadAzure { get; set; }

    [JsonPropertyName("extract")]
    public JobTaskExtractSettings? Extract { get; set; }

    [JsonPropertyName("restoreBackup")]
    public JobTaskRestoreDatabaseSettings? RestoreBackup { get; set; }

    [JsonPropertyName("uploadS3")]
    public JobTaskUploadS3Settings? UploadS3 { get; set; }

    [JsonPropertyName("uploadAzure")]
    public JobTaskUploadAzureSettings? UploadAzure { get; set; }
}

public class JobTaskDeleteS3Settings
{
    [JsonPropertyName("fileName")]
    public String? FileName { get; set; }

    [JsonPropertyName("region")]
    public String? Region { get; set; }

    [JsonPropertyName("bucket")]
    public String? Bucket { get; set; }

    [JsonPropertyName("accessKey")]
    public String? AccessKey { get; set; }

    [JsonPropertyName("secret")]
    [Protected]
    public String? Secret { get; set; }

    [JsonPropertyName("tag")]
    public String? Tag { get; set; }

    [JsonPropertyName("timeSpanAmount")]
    public Int32 TimeSpanAmount { get; set; }

    [JsonPropertyName("timeSpanType")]
    public String? TimeSpanType { get; set; }
}

public class JobTaskCreateDatabaseSettings
{
    [JsonPropertyName("databaseType")]
    public String? DatabaseType { get; set; }

    [JsonPropertyName("connectionString")]
    public String? ConnectionString { get; set; }

    [JsonPropertyName("connectionStringPassword")]
    [Protected]
    public String? ConnectionStringPassword { get; set; }

    [JsonPropertyName("backupType")]
    public String? BackupType { get; set; }

    [JsonPropertyName("fileName")]
    public String? FileName { get; set; }

    [JsonPropertyName("options")]
    public String? Options { get; set; }

    [JsonPropertyName("backupDefaultExclude")]
    public Boolean BackupDefaultExclude { get; set; }

    [JsonPropertyName("backupExcludeSystemDatabases")]
    public Boolean BackupExcludeSystemDatabases { get; set; }

    [JsonPropertyName("backupIncludeRegex")]
    public String? BackupIncludeRegex { get; set; }

    [JsonPropertyName("backupExcludeRegex")]
    public String? BackupExcludeRegex { get; set; }

    [JsonPropertyName("backupIncludeManual")]
    public String? BackupIncludeManual { get; set; }

    [JsonPropertyName("backupExcludeManual")]
    public String? BackupExcludeManual { get; set; }
}

public class JobTaskCompressSettings
{
    [JsonPropertyName("fileName")]
    public String? FileName { get; set; }

    [JsonPropertyName("archiveType")]
    public String? ArchiveType { get; set; }

    [JsonPropertyName("compressionLevel")]
    public String? CompressionLevel { get; set; }

    [JsonPropertyName("multithreadMode")]
    public String? MultithreadMode { get; set; }

    [JsonPropertyName("password")]
    [Protected]
    public String? Password { get; set; }
}

public class JobTaskDeleteFileSettings
{
    [JsonPropertyName("ignoreIfFileDoesNotExist")]
    public Boolean IgnoreIfFileDoesNotExist { get; set; }
}

public class JobTaskDownloadS3Settings
{
    [JsonPropertyName("fileName")]
    public String? FileName { get; set; }

    [JsonPropertyName("region")]
    public String? Region { get; set; }

    [JsonPropertyName("bucket")]
    public String? Bucket { get; set; }

    [JsonPropertyName("accessKey")]
    public String? AccessKey { get; set; }

    [JsonPropertyName("secret")]
    [Protected]
    public String? Secret { get; set; }
}

public class JobTaskDownloadAzureSettings
{
    [JsonPropertyName("blob")]
    public String? Blob { get; set; }

    [JsonPropertyName("fileName")]
    public String? FileName { get; set; }

    [JsonPropertyName("containerName")]
    public String? ContainerName { get; set; }

    [JsonPropertyName("connectionString")]
    [Protected]
    public String? ConnectionString { get; set; }

    [JsonPropertyName("restoreDefaultExclude")]
    public Boolean RestoreDefaultExclude { get; set; }

    [JsonPropertyName("restoreIncludeRegex")]
    public String? RestoreIncludeRegex { get; set; }

    [JsonPropertyName("restoreExcludeRegex")]
    public String? RestoreExcludeRegex { get; set; }

    [JsonPropertyName("restoreIncludeManual")]
    public String? RestoreIncludeManual { get; set; }

    [JsonPropertyName("restorepExcludeManual")]
    public String? RestoreExcludeManual { get; set; }
}

public class JobTaskExtractSettings
{
    [JsonPropertyName("fileName")]
    public String? FileName { get; set; }

    [JsonPropertyName("archiveType")]
    public String? ArchiveType { get; set; }

    [JsonPropertyName("multithreadMode")]
    public String? MultithreadMode { get; set; }

    [JsonPropertyName("password")]
    [Protected]
    public String? Password { get; set; }
}

public class JobTaskRestoreDatabaseSettings
{
    [JsonPropertyName("databaseType")]
    public String? DatabaseType { get; set; }

    [JsonPropertyName("connectionString")]
    public String? ConnectionString { get; set; }

    [JsonPropertyName("connectionStringPassword")]
    [Protected]
    public String? ConnectionStringPassword { get; set; }

    [JsonPropertyName("databaseName")]
    public String? DatabaseName { get; set; }

    [JsonPropertyName("databaseLocation")]
    public String? DatabaseLocation { get; set; }

    [JsonPropertyName("options")]
    public String? Options { get; set; }
}

public class JobTaskUploadS3Settings
{
    [JsonPropertyName("fileName")]
    public String? FileName { get; set; }

    [JsonPropertyName("region")]
    public String? Region { get; set; }

    [JsonPropertyName("bucket")]
    public String? Bucket { get; set; }

    [JsonPropertyName("accessKey")]
    public String? AccessKey { get; set; }

    [JsonPropertyName("secret")]
    [Protected]
    public String? Secret { get; set; }

    [JsonPropertyName("tag")]
    public String? Tag { get; set; }
}
    
public class JobTaskUploadAzureSettings
{
    [JsonPropertyName("fileName")]
    public String? FileName { get; set; }

    [JsonPropertyName("containerName")]
    public String? ContainerName { get; set; }

    [JsonPropertyName("connectionString")]
    [Protected]
    public String? ConnectionString { get; set; }

    [JsonPropertyName("tag")]
    public String? Tag { get; set; }
}