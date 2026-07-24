namespace Application.WorkCalendars.GetCalendarImpactAlerts;

public class CalendarImpactAlertItem
{
    public DateOnly Date { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string DayType { get; set; } = string.Empty;
    public int AffectedLeaveRequestsCount { get; set; }
    public string ImpactSummary { get; set; } = string.Empty;
}
