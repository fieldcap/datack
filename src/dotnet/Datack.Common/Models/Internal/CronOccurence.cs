using System;
using Datack.Common.Enums;

namespace Datack.Common.Models.Internal
{
    public class CronOccurrence
    {
        public DateTimeOffset DateTime { get; set; }
        public BackupType BackupType { get; set; }
    }
}
