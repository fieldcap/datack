using System.Text.RegularExpressions;
using Datack.Common.Models.Internal;

namespace Datack.Common.Helpers;

public static class DatabaseHelper
{
    private static readonly String[] SystemDatabases =
    {
        "master", "tempdb", "model", "msdb"
    };

    public static List<DatabaseTestResult> FilterDatabases(IList<Database>? databases,
                                                           Boolean backupDefaultExclude,
                                                           Boolean backupExcludeSystemDatabases,
                                                           String? backupIncludeRegex,
                                                           String? backupExcludeRegex,
                                                           String? backupIncludeManual,
                                                           String? backupExcludeManual,
                                                           String? backupType)
    {
        databases ??= new List<Database>();

        var resultList = new List<DatabaseTestResult>();

        var excludeManualList = new List<String>();
        var includeManualList = new List<String>();

        if (!String.IsNullOrWhiteSpace(backupIncludeManual))
        {
            includeManualList = backupIncludeManual.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        }

        if (!String.IsNullOrWhiteSpace(backupExcludeManual))
        {
            excludeManualList = backupExcludeManual.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
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
            else if (!database.HasFullbackup && backupType != "Full")
            {
                result.HasNoFullBackup = true;
            }
            else if (includeManualList.Contains(database.DatabaseName))
            {
                result.IsManualIncluded = true;
            }
            else if (excludeManualList.Contains(database.DatabaseName))
            {
                result.IsManualExcluded = true;
            }
            else if (backupExcludeSystemDatabases && SystemDatabases.Contains(database.DatabaseName))
            {
                result.IsSystemDatabase = true;
            }
            else if (!String.IsNullOrWhiteSpace(backupIncludeRegex) && Regex.IsMatch(database.DatabaseName, backupIncludeRegex))
            {
                result.IsRegexIncluded = true;
            }
            else if (!String.IsNullOrWhiteSpace(backupExcludeRegex) && Regex.IsMatch(database.DatabaseName, backupExcludeRegex))
            {
                result.IsRegexExcluded = true;
            }
            else if (backupDefaultExclude)
            {
                result.IsBackupDefaultExcluded = true;
            }

            resultList.Add(result);
        }

        return resultList;
    }
}