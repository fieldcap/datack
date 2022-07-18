namespace Datack.Common.Models.Internal;

public class DatabaseTestResult
{
    public String DatabaseName { get; set; }
    public Boolean HasNoAccess { get; set; }
    public Boolean IsManualIncluded { get; set; }
    public Boolean IsManualExcluded { get; set; }
    public Boolean IsSystemDatabase { get; set; }
    public Boolean IsRegexIncluded { get; set; }
    public Boolean IsRegexExcluded { get; set; }
    public Boolean IsBackupDefaultExcluded { get; set; }

    public Boolean Include => !HasNoAccess && !IsManualExcluded && !IsRegexExcluded && !IsSystemDatabase && !IsBackupDefaultExcluded;
}