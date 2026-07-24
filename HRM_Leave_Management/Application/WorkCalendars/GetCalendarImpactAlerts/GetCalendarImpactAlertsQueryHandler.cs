using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.LeaveRequests;
using Domain.WorkCalendars;
using Microsoft.EntityFrameworkCore;

namespace Application.WorkCalendars.GetCalendarImpactAlerts;

internal sealed class GetCalendarImpactAlertsQueryHandler
    : IQueryHandler<GetCalendarImpactAlertsQuery, List<CalendarImpactAlertItem>>
{
    private readonly IWorkCalendarDayRepository _workCalendarDayRepository;
    private readonly ILeaveRequestRepository _leaveRequestRepository;

    public GetCalendarImpactAlertsQueryHandler(
        IWorkCalendarDayRepository workCalendarDayRepository,
        ILeaveRequestRepository leaveRequestRepository)
    {
        _workCalendarDayRepository = workCalendarDayRepository;
        _leaveRequestRepository = leaveRequestRepository;
    }

    public async Task<Result<List<CalendarImpactAlertItem>>> Handle(
        GetCalendarImpactAlertsQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var endDate = today.AddDays(30);

        var yearDays = await _workCalendarDayRepository.GetActiveByYearAsync(today.Year, cancellationToken);
        if (today.Year != endDate.Year)
        {
            var nextYearDays = await _workCalendarDayRepository.GetActiveByYearAsync(endDate.Year, cancellationToken);
            yearDays.AddRange(nextYearDays);
        }

        var upcomingHolidays = yearDays
            .Where(d => d.Date >= today && d.Date <= endDate &&
                        (d.DayType == CalendarDayType.PublicHoliday || d.DayType == CalendarDayType.CompanyCustomNonWorkingDay))
            .OrderBy(d => d.Date)
            .ToList();

        if (!upcomingHolidays.Any())
        {
            return Result.Success(new List<CalendarImpactAlertItem>());
        }

        var activeLeaveRequests = await _leaveRequestRepository.GetEntitiesAsQueryable()
            .AsNoTracking()
            .Where(lr => lr.Employee.IsActive &&
                         (lr.Status == LeaveRequestStatus.Pending || lr.Status == LeaveRequestStatus.Approved) &&
                         lr.StartDate <= endDate && lr.EndDate >= today)
            .ToListAsync(cancellationToken);

        var alerts = new List<CalendarImpactAlertItem>();

        foreach (var holiday in upcomingHolidays)
        {
            var overlappingCount = activeLeaveRequests
                .Count(lr => lr.StartDate <= holiday.Date && lr.EndDate >= holiday.Date);

            alerts.Add(new CalendarImpactAlertItem
            {
                Date = holiday.Date,
                EventName = holiday.Description ?? (holiday.DayType == CalendarDayType.PublicHoliday ? "Public Holiday" : "Company Non-Working Day"),
                DayType = holiday.DayType == CalendarDayType.PublicHoliday ? "Public Holiday" : "Rest Day",
                AffectedLeaveRequestsCount = overlappingCount,
                ImpactSummary = overlappingCount > 0
                    ? $"{overlappingCount} leave request(s) overlap this holiday period"
                    : "No leave request overlaps"
            });
        }

        return Result.Success(alerts);
    }
}
