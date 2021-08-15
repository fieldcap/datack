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

        public static List<DatabaseListTestResult> FilterDatabases(IList<DatabaseList> databases,
                                                                   Boolean excludeSystemDatabases,
                                                                   String includeRegex,
                                                                   String excludeRegex,
                                                                   String includeManual,
                                                                   String excludeManual,
                                                                   Boolean backupDefaultExclude)
        {
            databases ??= new List<DatabaseList>();

            var resultList = new List<DatabaseListTestResult>();

            var excludeManualList = new List<String>();
            var includeManualList = new List<String>();

            if (!String.IsNullOrWhiteSpace(includeManual))
            {
                includeManualList = includeManual.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            }

            if (!String.IsNullOrWhiteSpace(excludeManual))
            {
                excludeManualList = excludeManual.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            }

            foreach (var database in databases)
            {
                var result = new DatabaseListTestResult
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
                else if (excludeSystemDatabases && SystemDatabases.Contains(database.DatabaseName))
                {
                    result.IsSystemDatabase = true;
                }
                else if (!String.IsNullOrWhiteSpace(includeRegex) && Regex.IsMatch(database.DatabaseName, includeRegex))
                {
                    result.IsRegexIncluded = true;
                }
                else if (!String.IsNullOrWhiteSpace(excludeRegex) && Regex.IsMatch(database.DatabaseName, excludeRegex))
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
}
