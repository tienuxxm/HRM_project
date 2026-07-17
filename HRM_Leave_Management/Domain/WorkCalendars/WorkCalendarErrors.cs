using Domain.Abstractions;

namespace Domain.WorkCalendars;

public static class WorkCalendarErrors
{
    public static readonly Error PastEditingNotAllowed = new(
        "WorkCalendar.PastEditingNotAllowed",
        "Configuring calendar days for past dates is not allowed");
}
