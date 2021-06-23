using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Datack.Common.Helpers
{
    public static class DatabaseHelper
    {
        private static readonly String[] SystemDatabases =
        {
            "master", "tempdb", "model", "msdb"
        };

        public static (List<String> systemList, List<String> includeRegexList, List<String> excludeRegexList, List<String> includeManualList, List<String> excludeManualList)
            FilterDatabases(IList<String> databases,
                            Boolean excludeSystemDatabases,
                            String includeRegex,
                            String excludeRegex,
                            String includeManual,
                            String excludeManual)
        {
            databases ??= new List<String>();

            var systemList = new List<String>();
            var includeRegexList = new List<String>();
            var excludeRegexList = new List<String>();
            var includeManualList = new List<String>();
            var excludeManualList = new List<String>();

            if (!String.IsNullOrWhiteSpace(includeManual))
            {
                var m = includeManual.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                foreach (var database in databases)
                {
                    if (m.Contains(database))
                    {
                        includeManualList.Add(database);
                    }
                }
            }

            if (!String.IsNullOrWhiteSpace(excludeManual))
            {
                var m = excludeManual.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                foreach (var database in databases)
                {
                    if (m.Contains(database))
                    {
                        excludeManualList.Add(database);
                    }
                }
            }

            if (excludeSystemDatabases)
            {
                systemList = databases.Where(m => SystemDatabases.Contains(m)).ToList();
            }

            if (!String.IsNullOrWhiteSpace(includeRegex))
            {
                foreach (var database in databases)
                {
                    if (Regex.IsMatch(database, includeRegex))
                    {
                        includeRegexList.Add(database);
                    }
                }
            }

            if (!String.IsNullOrWhiteSpace(excludeRegex))
            {
                foreach (var database in databases)
                {
                    if (Regex.IsMatch(database, excludeRegex))
                    {
                        excludeRegexList.Add(database);
                    }
                }
            }

            return (systemList, includeRegexList, excludeRegexList, includeManualList, excludeManualList);
        }
    }
}
