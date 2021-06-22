using System;
using System.Text.Json.Serialization;

namespace Datack.Common.Models.Internal
{
    public class StepSettings
    {
        [JsonPropertyName("createBackup")]
        public StepCreateDatabaseSettings CreateBackup { get;set; }
    }

    public class StepCreateDatabaseSettings
    {
        [JsonPropertyName("backupAllNonSystemDatabases")]
        public Boolean BackupAllNonSystemDatabases{ get;set; }
    }
}
