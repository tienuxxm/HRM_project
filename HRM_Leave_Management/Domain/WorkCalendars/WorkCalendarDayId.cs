namespace Domain.WorkCalendars;

public record WorkCalendarDayId(Guid Value)
{
    public static WorkCalendarDayId New() => new(Guid.NewGuid());
}
