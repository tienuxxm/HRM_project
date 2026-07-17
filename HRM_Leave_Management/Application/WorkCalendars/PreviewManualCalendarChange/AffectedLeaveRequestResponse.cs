namespace Application.WorkCalendars.PreviewManualCalendarChange;

public sealed record AffectedLeaveRequestResponse(
    Guid LeaveRequestId,
    string EmployeeName,
    string LeaveTypeName,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal OldDuration,
    decimal NewDuration,
    string OldStatus,
    string NewStatus);
