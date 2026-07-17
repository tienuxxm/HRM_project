using Application.Abstractions.Messaging;

namespace Application.WorkCalendars.PreviewManualCalendarChange;

public sealed record PreviewManualCalendarChangeQuery(
    DateOnly Date,
    string DayType,
    string WorkShift,
    bool IsActive) : IQuery<List<AffectedLeaveRequestResponse>>;
