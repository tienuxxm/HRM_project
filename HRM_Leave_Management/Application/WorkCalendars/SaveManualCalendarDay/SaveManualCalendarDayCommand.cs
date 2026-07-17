using Application.Abstractions.Messaging;

namespace Application.WorkCalendars.SaveManualCalendarDay;

public sealed record SaveManualCalendarDayCommand(
    DateOnly Date,
    string DayType,
    string WorkShift,
    string? Description,
    bool IsActive) : ICommand;
