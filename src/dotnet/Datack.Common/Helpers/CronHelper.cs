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

        public static IList<CronOccurence> GetNextOccurrences(String fullCron, String diffCron, String logCron, TimeSpan timeSpan)
        {
            var fullOccurences = GetNextOccurrences(fullCron, timeSpan);
            var diffOccurences = GetNextOccurrences(diffCron, timeSpan);
            var logOccurences = GetNextOccurrences(logCron, timeSpan);

            var result = new List<CronOccurence>();

            foreach (var occurence in logOccurences)
            {
                result.Add(new CronOccurence
                {
                    BackupType = BackupType.Log,
                    DateTime = occurence
                });
            }

            foreach (var occurence in diffOccurences)
            {
                var existingOccurence = result.FirstOrDefault(m => m.DateTime == occurence);

                if (existingOccurence != null)
                {
                    existingOccurence.BackupType = BackupType.Diff;
                }
                else
                {
                    result.Add(new CronOccurence
                    {
                        BackupType = BackupType.Diff,
                        DateTime = occurence
                    });
                }
            }

            foreach (var occurence in fullOccurences)
            {
                var existingOccurence = result.FirstOrDefault(m => m.DateTime == occurence);

                if (existingOccurence != null)
                {
                    existingOccurence.BackupType = BackupType.Full;
                }
                else
                {
                    result.Add(new CronOccurence
                    {
                        BackupType = BackupType.Full,
                        DateTime = occurence
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
                var occurences = parsedExpression.GetOccurrences(from, to, TimeZoneInfo.Local, true, true);

                return occurences.ToList();
            }
            catch
            {
                return new List<DateTimeOffset>();
            }
        }
    }
}
