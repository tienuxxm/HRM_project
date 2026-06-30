using Application.Abstractions.Clock;

namespace Infrastructure.Clock;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime TimeStampToUtc(long timestamp)
    {
        var seconds = timestamp / 1000;
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var dateTime = epoch.AddSeconds(seconds);

        var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        // Convert UTC time to Vietnam time
        var vnTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, vnTimeZone);

        return vnTime;
    }

    public DateTime ToVnTime(DateTime time)
    {
        var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(time, vietnamTimeZone);
    }
}