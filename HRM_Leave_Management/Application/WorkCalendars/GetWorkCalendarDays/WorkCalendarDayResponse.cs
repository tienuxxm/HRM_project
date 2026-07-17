namespace Application.WorkCalendars.GetWorkCalendarDays;

public sealed class WorkCalendarDayResponse
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public string DayType { get; set; } = null!;
    public string WorkShift { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
