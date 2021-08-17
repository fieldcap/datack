using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByteSizeLib;
using Datack.Common.Enums;
using Datack.Common.Extensions;
using Datack.Common.Helpers;
using Datack.Common.Models.Data;

namespace Datack.Agent.Services.Tasks
{
    public class CreateBackupTask : BaseTask
    {
        private const Int32 Parallel = 2;

        private readonly DatabaseAdapter _databaseAdapter;

        public CreateBackupTask(DatabaseAdapter databaseAdapter)
        {
            _databaseAdapter = databaseAdapter;
        }

        public override async Task<IList<StepLog>> Setup(Job job, Step step, BackupType backupType, Guid jobLogId)
        {
            var databases = new List<DatabaseStep>();

            OnProgress("Getting list of databases");

            var allDatabaseList = await _databaseAdapter.GetDatabaseList();

            OnProgress($"Found total of {allDatabaseList.Count} databases");

            var filteredDatabaseList = DatabaseHelper.FilterDatabases(allDatabaseList, step.Settings.CreateBackup);

            var databaseList = filteredDatabaseList.Where(m => m.Include).ToList();

            OnProgress($"Found total of {databaseList.Count} eligible databases");

            var databaseFileSizeList = await _databaseAdapter.GetFileList();

            foreach (var database in databaseList)
            {
                var files = databaseFileSizeList.Where(m => m.DatabaseName == database.DatabaseName).ToList();

                Int64 size = 0;

                if (files.Count > 0)
                {
                    size = files.Sum(m => m.Size);
                }

                databases.Add(new DatabaseStep
                {
                    DatabaseName = database.DatabaseName,
                    Size = size
                });
            }

            var totalSize = (Int64) databases.Sum(m => m.Size);

            OnProgress($"Estimated total size {ByteSize.FromKiloBytes(totalSize)}");

            var batches = databases.Batch(Parallel);

            var results = new List<StepLog>();

            var batchIndex = 0;
            foreach (var batch in batches)
            {
                foreach (var database in batch)
                {
                    var stepLog = new StepLog
                    {
                        StepLogId = Guid.NewGuid(),
                        StepId = step.StepId,
                        JobLogId = jobLogId,
                        DatabaseName = database.DatabaseName,
                        Queue = batchIndex,
                        Type = step.Type,
                        Settings = step.Settings
                    };

                    results.Add(stepLog);
                }

                batchIndex++;
            }

            return results;
            /*var backupName = $"{database} Full Backup";
            var query = $@"BACKUP DATABASE [{database}] TO  DISK = N'{dbFilePath}' WITH NOFORMAT, INIT,  NAME = N'{backupName}', SKIP, NOREWIND, NOUNLOAD,  STATS = 10";*/
        }

        public override async Task Run(List<StepLog> queue)
        {
            throw new NotImplementedException();
        }
    }

    public class DatabaseStep
    {
        public String DatabaseName { get; set; }
        public Decimal Size { get; set; }
    }
}
