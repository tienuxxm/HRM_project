using System.Globalization;

namespace Domain.Extension;

public static class StringExtentions
{
    public static DateTime StringToDateTimeUtc(this string? value, bool hasTime = false)
    {
        var format = "dd/MM/yyyy";
        if (hasTime)
        {
            format += " HH:mm";
        }

        IFormatProvider culture = new CultureInfo("vi-VN", true);

        if (string.IsNullOrEmpty(value))
            return DateTime.UtcNow;
        var date = DateTimeOffset.ParseExact(value, format, culture).UtcDateTime;
        return DateTime.SpecifyKind(date, DateTimeKind.Utc);
    }
}