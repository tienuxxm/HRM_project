using Application.Abstractions.Messaging;

namespace Application.WorkCalendars.GetCalendarImpactAlerts;

public record GetCalendarImpactAlertsQuery() : IQuery<List<CalendarImpactAlertItem>>;
