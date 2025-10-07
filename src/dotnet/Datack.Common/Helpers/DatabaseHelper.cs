using System.Text.RegularExpressions;
using Datack.Common.Models.Internal;

namespace Datack.Common.Helpers;

public static class FileHelper
{
    public static List<DatabaseTestResult> FilterFiles(IList<BackupFile>? files,
                                                       Boolean restoreDefaultExclude,
                                                       String? restoreIncludeRegex,
                                                       String? restoreExcludeRegex,
                                                       String? restoreIncludeManual,
                                                       String? restoreExcludeManual)
    {
        files ??= [];

        var resultList = new List<DatabaseTestResult>();

        var excludeManualList = new List<String>();
        var includeManualList = new List<String>();

        if (!String.IsNullOrWhiteSpace(restoreIncludeManual))
        {
            includeManualList = [.. restoreIncludeManual.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
        }

        if (!String.IsNullOrWhiteSpace(restoreExcludeManual))
        {
            excludeManualList = [.. restoreExcludeManual.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
        }

        foreach (var file in files)
        {
            var result = new DatabaseTestResult
            {
                DatabaseName = file.FileName
            };

            if (includeManualList.Contains(file.FileName))
            {
                result.IsManualIncluded = true;
            }
            else if (excludeManualList.Contains(file.FileName))
            {
                result.IsManualExcluded = true;
            }
            else if (!String.IsNullOrWhiteSpace(restoreIncludeRegex) && Regex.IsMatch(file.FileName, restoreIncludeRegex))
            {
                result.IsRegexIncluded = true;
            }
            else if (!String.IsNullOrWhiteSpace(restoreExcludeRegex) && Regex.IsMatch(file.FileName, restoreExcludeRegex))
            {
                result.IsRegexExcluded = true;
            }
            else if (restoreDefaultExclude)
            {
                result.IsBackupDefaultExcluded = true;
            }

            resultList.Add(result);
        }

        return resultList;
    }
}