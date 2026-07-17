using Domain.Abstractions;

namespace Domain.WorkCalendars;

public class WorkCalendarDay : Entity<WorkCalendarDayId>
{
    private WorkCalendarDay(
        WorkCalendarDayId id,
        DateOnly date,
        CalendarDayType dayType,
        WorkShiftType workShift,
        string? description,
        bool isActive,
        Guid createdBy,
        DateTime createdAt)
    {
        Id = id;
        Date = date;
        DayType = dayType;
        WorkShift = workShift;
        Description = description;
        IsActive = isActive;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
    }

    private WorkCalendarDay()
    {
    }

    public DateOnly Date { get; private set; }
    public CalendarDayType DayType { get; private set; }
    public WorkShiftType WorkShift { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static WorkCalendarDay Create(
        DateOnly date,
        CalendarDayType dayType,
        WorkShiftType workShift,
        string? description,
        Guid createdBy)
    {
        return new WorkCalendarDay(
            WorkCalendarDayId.New(),
            date,
            dayType,
            workShift,
            description,
            isActive: true,
            createdBy: createdBy,
            createdAt: DateTime.UtcNow);
    }

    public void Update(
        CalendarDayType dayType,
        WorkShiftType workShift,
        string? description)
    {
        DayType = dayType;
        WorkShift = workShift;
        Description = description;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
