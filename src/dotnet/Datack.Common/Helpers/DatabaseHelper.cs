using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Datack.Common.Models.Internal;

namespace Datack.Common.Helpers
{
    public static class DatabaseHelper
    {
        private static readonly String[] SystemDatabases =
        {
            "master", "tempdb", "model", "msdb"
        };

        public static List<DatabaseTestResult> FilterDatabases(IList<Database> databases, StepCreateDatabaseSettings settings)
        {
            databases ??= new List<Database>();

            var resultList = new List<DatabaseTestResult>();

            var excludeManualList = new List<String>();
            var includeManualList = new List<String>();

            if (!String.IsNullOrWhiteSpace(settings.BackupIncludeManual))
            {
                includeManualList = settings.BackupIncludeManual.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            }

            if (!String.IsNullOrWhiteSpace(settings.BackupExcludeManual))
            {
                excludeManualList = settings.BackupExcludeManual.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            }

            foreach (var database in databases)
            {
                var result = new DatabaseTestResult
                {
                    DatabaseName = database.DatabaseName
                };

                if (!database.HasAccess)
                {
                    result.HasNoAccess = true;
                }
                else if (includeManualList.Contains(database.DatabaseName))
                {
                    result.IsManualIncluded = true;
                }
                else if (excludeManualList.Contains(database.DatabaseName))
                {
                    result.IsManualExcluded = true;
                }
                else if (settings.BackupExcludeSystemDatabases && SystemDatabases.Contains(database.DatabaseName))
                {
                    result.IsSystemDatabase = true;
                }
                else if (!String.IsNullOrWhiteSpace(settings.BackupIncludeRegex) && Regex.IsMatch(database.DatabaseName, settings.BackupIncludeRegex))
                {
                    result.IsRegexIncluded = true;
                }
                else if (!String.IsNullOrWhiteSpace(settings.BackupExcludeRegex) && Regex.IsMatch(database.DatabaseName, settings.BackupExcludeRegex))
                {
                    result.IsRegexExcluded = true;
                }
                else if (settings.BackupDefaultExclude)
                {
                    result.IsBackupDefaultExcluded = true;
                }

                resultList.Add(result);
            }

            return resultList;
        }
    }
}
