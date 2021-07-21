using System;
using System.Collections.Generic;
using System.Linq;
using CronExpressionDescriptor;
using Cronos;
using Datack.Common.Models.Internal;

namespace Datack.Common.Helpers
{
    public static class CronHelper
    {
        public static CronParseResult ParseCron(String cron)
        {
            var result = new CronParseResult
            {
                Next = new List<String>()
            };

            if (String.IsNullOrWhiteSpace(cron))
            {
                result.Description = "Never";
                return result;
            }

            try
            {
                var now = DateTime.UtcNow;
                now = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, DateTimeKind.Utc);

                var parsedExpression = CronExpression.Parse(cron);
                var occurences = parsedExpression.GetOccurrences(now, now.AddMonths(12), true, true);
                result.Next = occurences.Take(10).Select(m => m.ToLocalTime().ToString("dddd d MMMM yyyy HH:mm")).ToList();

                var options = new Options
                {
                    Locale = "en"
                };

                result.Description = ExpressionDescriptor.GetDescription(cron, options);
            }
            catch (Exception ex)
            {
                result.Description = $"Invalid cron expression: {ex.Message}";

                return result;
            }
            return result;
        }
    }
}
