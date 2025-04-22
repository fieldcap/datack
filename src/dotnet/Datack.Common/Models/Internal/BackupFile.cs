namespace Datack.Common.Models.Internal;

public class BackupFile
{
    public required String FileName { get; set; }
    public String? DatabaseName { get;set; }
    public String? BackupType { get; set; }
    public DateTimeOffset? DateTime { get; set; }
}
