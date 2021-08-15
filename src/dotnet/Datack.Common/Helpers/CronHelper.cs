using System;
using System.Collections.Generic;
using System.Linq;
using CronExpressionDescriptor;
using Cronos;
using Datack.Common.Enums;
using Datack.Common.Models.Internal;

namespace Datack.Common.Helpers
{
    public static class CronHelper
    {
        public static String ParseCron(String cron)
        {
            if (String.IsNullOrWhiteSpace(cron))
            {
                return "Never";
            }

            try
            {
                var options = new Options
                {
                    Locale = "en"
                };

                return ExpressionDescriptor.GetDescription(cron, options);
            }
            catch (Exception ex)
            {
                return $"Invalid cron expression: {ex.Message}";
            }
        }

        public static IList<CronOccurrence> GetNextOccurrences(String fullCron, String diffCron, String logCron, TimeSpan timeSpan)
        {
            var fullOccurrences = GetNextOccurrences(fullCron, timeSpan);
            var diffOccurrences = GetNextOccurrences(diffCron, timeSpan);
            var logOccurrences = GetNextOccurrences(logCron, timeSpan);

            var result = new List<CronOccurrence>();

            foreach (var occurrence in logOccurrences)
            {
                result.Add(new CronOccurrence
                {
                    BackupType = BackupType.Log,
                    DateTime = occurrence
                });
            }

            foreach (var occurrence in diffOccurrences)
            {
                var existingOccurrence = result.FirstOrDefault(m => m.DateTime == occurrence);

                if (existingOccurrence != null)
                {
                    existingOccurrence.BackupType = BackupType.Diff;
                }
                else
                {
                    result.Add(new CronOccurrence
                    {
                        BackupType = BackupType.Diff,
                        DateTime = occurrence
                    });
                }
            }

            foreach (var occurrence in fullOccurrences)
            {
                var existingOccurrence = result.FirstOrDefault(m => m.DateTime == occurrence);

                if (existingOccurrence != null)
                {
                    existingOccurrence.BackupType = BackupType.Full;
                }
                else
                {
                    result.Add(new CronOccurrence
                    {
                        BackupType = BackupType.Full,
                        DateTime = occurrence
                    });
                }
            }

            return result.OrderBy(m => m.DateTime).ToList();
        }

        private static IList<DateTimeOffset> GetNextOccurrences(String cron, TimeSpan timeSpan)
        {
            if (String.IsNullOrWhiteSpace(cron))
            {
                return new List<DateTimeOffset>();
            }

            try
            {
                var now = DateTime.UtcNow;
                var from = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, TimeZoneInfo.Local.BaseUtcOffset);
                var to = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, TimeZoneInfo.Local.BaseUtcOffset).Add(timeSpan);

                var parsedExpression = CronExpression.Parse(cron);
                var occurrences = parsedExpression.GetOccurrences(from, to, TimeZoneInfo.Local, true, true);

                return occurrences.ToList();
            }
            catch
            {
                return new List<DateTimeOffset>();
            }
        }
    }
}
