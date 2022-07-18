using CronExpressionDescriptor;
using Cronos;

namespace Datack.Common.Helpers;

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
        
    public static IList<DateTimeOffset> GetNextOccurrences(String cron, TimeSpan timeSpan)
    {
        if (String.IsNullOrWhiteSpace(cron))
        {
            return new List<DateTimeOffset>();
        }

        try
        {
            var now = DateTimeOffset.Now;
            var from = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, TimeZoneInfo.Local.GetUtcOffset(now));
            var to = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, TimeZoneInfo.Local.GetUtcOffset(now)).Add(timeSpan);

            var parsedExpression = CronExpression.Parse(cron);
            var occurrences = parsedExpression.GetOccurrences(from, to, TimeZoneInfo.Local, true, true);

            return occurrences.ToList();
        }
        catch
        {
            return new List<DateTimeOffset>();
        }
    }

    public static DateTimeOffset? GetNextOccurrence(String cron, DateTimeOffset dateTime)
    {
        if (String.IsNullOrWhiteSpace(cron))
        {
            return null;
        }

        try
        {
            var parsedExpression = CronExpression.Parse(cron);
            var result = parsedExpression.GetNextOccurrence(dateTime, TimeZoneInfo.Local, true);

            return result;
        }
        catch
        {
            return null;
        }
    }
}