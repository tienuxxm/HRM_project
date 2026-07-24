using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.WorkCalendars;

namespace Application.WorkCalendars.GetUpcomingHolidays;

/// <summary>
/// Dashboard W8 handler: Returns the next 5 upcoming non-working days from the work calendar.
/// Non-working days = PublicHoliday (1) or CompanyCustomNonWorkingDay (2).
/// Allows crossing year boundary (e.g., December → January next year).
/// Uses existing IWorkCalendarDayRepository.GetActiveByYearAsync (no Domain modification needed).
/// Read-only. No DB mutation.
/// </summary>
internal sealed class GetUpcomingHolidaysQueryHandler
    : IQueryHandler<GetUpcomingHolidaysQuery, List<UpcomingHolidayItem>>
{
    private readonly IWorkCalendarDayRepository _workCalendarDayRepository;

    public GetUpcomingHolidaysQueryHandler(IWorkCalendarDayRepository workCalendarDayRepository)
    {
        _workCalendarDayRepository = workCalendarDayRepository;
    }

    public async Task<Result<List<UpcomingHolidayItem>>> Handle(
        GetUpcomingHolidaysQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        int currentYear = today.Year;

        // Get current year's active calendar days
        var currentYearDays = await _workCalendarDayRepository.GetActiveByYearAsync(currentYear, cancellationToken);

        // Filter non-working days from today onwards
        var upcomingDays = currentYearDays
            .Where(d => d.Date >= today &&
                        (d.DayType == CalendarDayType.PublicHoliday ||
                         d.DayType == CalendarDayType.CompanyCustomNonWorkingDay))
            .OrderBy(d => d.Date)
            .ToList();

        // If we need more entries, check next year too (cross-year boundary support)
        if (upcomingDays.Count < 5)
        {
            var nextYearDays = await _workCalendarDayRepository.GetActiveByYearAsync(currentYear + 1, cancellationToken);
            var nextYearHolidays = nextYearDays
                .Where(d => d.DayType == CalendarDayType.PublicHoliday ||
                            d.DayType == CalendarDayType.CompanyCustomNonWorkingDay)
                .OrderBy(d => d.Date)
                .ToList();

            upcomingDays.AddRange(nextYearHolidays);
        }

        var result = upcomingDays
            .Take(5)
            .Select(d => new UpcomingHolidayItem
            {
                Date = d.Date,
                DayName = d.Date.DayOfWeek.ToString(),
                DayType = d.DayType == CalendarDayType.PublicHoliday ? "Public Holiday" : "Company Non-Working Day",
                Description = d.Description
            })
            .ToList();

        return Result.Success(result);
    }
}
