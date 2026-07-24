using Application.Abstractions.Messaging;

namespace Application.WorkCalendars.GetUpcomingHolidays;

/// <summary>
/// Dashboard W8: Retrieves the next 5 upcoming non-working days (holidays) from the work calendar.
/// Global scope (organization-wide). Read-only query.
/// </summary>
public sealed record GetUpcomingHolidaysQuery() : IQuery<List<UpcomingHolidayItem>>;

public sealed class UpcomingHolidayItem
{
    public DateOnly Date { get; init; }
    public string DayName { get; init; } = string.Empty;
    public string DayType { get; init; } = string.Empty;
    public string? Description { get; init; }
}
