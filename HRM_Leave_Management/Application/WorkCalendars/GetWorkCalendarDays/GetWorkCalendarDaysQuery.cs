using Application.Abstractions.Messaging;

namespace Application.WorkCalendars.GetWorkCalendarDays;

public sealed record GetWorkCalendarDaysQuery(int? Year, int? Month) : IQuery<List<WorkCalendarDayResponse>>;
