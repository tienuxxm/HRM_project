using Domain.Abstractions;
using Domain.LeaveRequests;

namespace Application.WorkCalendars;

public interface IWorkCalendarService
{
    Task<Result<decimal>> CalculateLeaveDurationAsync(
        DateOnly startDate,
        DateOnly endDate,
        LeaveDayPart startDayPart,
        LeaveDayPart endDayPart,
        CancellationToken cancellationToken);
}
