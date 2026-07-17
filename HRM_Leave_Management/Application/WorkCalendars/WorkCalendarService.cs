using Domain.Abstractions;
using Domain.LeaveRequests;
using Domain.WorkCalendars;

namespace Application.WorkCalendars;

public class WorkCalendarService : IWorkCalendarService
{
    private readonly IWorkCalendarDayRepository _workCalendarDayRepository;

    public WorkCalendarService(IWorkCalendarDayRepository workCalendarDayRepository)
    {
        _workCalendarDayRepository = workCalendarDayRepository;
    }

    public async Task<Result<decimal>> CalculateLeaveDurationAsync(
        DateOnly startDate,
        DateOnly endDate,
        LeaveDayPart startDayPart,
        LeaveDayPart endDayPart,
        CancellationToken cancellationToken)
    {
        if (startDate > endDate)
        {
            return Result.Failure<decimal>(LeaveRequestErrors.DateOrderInvalid);
        }

        if (startDate.Year != endDate.Year)
        {
            return Result.Failure<decimal>(LeaveRequestErrors.CrossYearNotAllowed);
        }

        if (startDate == endDate && startDayPart != endDayPart)
        {
            return Result.Failure<decimal>(LeaveRequestErrors.DayPartMismatch);
        }

        int year = startDate.Year;
        var activeDays = await _workCalendarDayRepository.GetActiveByYearAsync(year, cancellationToken);
        var configDict = activeDays.ToDictionary(d => d.Date);

        decimal totalDuration = 0;
        var currentDate = startDate;

        while (currentDate <= endDate)
        {
            WorkShiftType shiftType;
            if (configDict.TryGetValue(currentDate, out var calendarDay))
            {
                shiftType = calendarDay.WorkShift;
            }
            else
            {
                var dayOfWeek = currentDate.DayOfWeek;
                if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
                {
                    shiftType = WorkShiftType.None;
                }
                else
                {
                    shiftType = WorkShiftType.FullDay;
                }
            }

            decimal contribution = 0;

            if (startDate == endDate)
            {
                // Yêu cầu cùng ngày (Same-day request)
                var part = startDayPart;
                if (shiftType == WorkShiftType.None)
                {
                    contribution = 0.0m;
                }
                else if (shiftType == WorkShiftType.FullDay)
                {
                    if (part == LeaveDayPart.FullDay)
                    {
                        contribution = 1.0m;
                    }
                    else
                    {
                        contribution = 0.5m; // Morning hoặc Afternoon
                    }
                }
                else if (shiftType == WorkShiftType.MorningOnly)
                {
                    if (part == LeaveDayPart.FullDay || part == LeaveDayPart.Morning)
                    {
                        contribution = 0.5m;
                    }
                    else
                    {
                        return Result.Failure<decimal>(LeaveRequestErrors.InvalidShiftRegistration);
                    }
                }
                else if (shiftType == WorkShiftType.AfternoonOnly)
                {
                    if (part == LeaveDayPart.FullDay || part == LeaveDayPart.Afternoon)
                    {
                        contribution = 0.5m;
                    }
                    else
                    {
                        return Result.Failure<decimal>(LeaveRequestErrors.InvalidShiftRegistration);
                    }
                }
            }
            else
            {
                // Yêu cầu nhiều ngày (Multi-day request)
                if (currentDate == startDate)
                {
                    // Ngày bắt đầu
                    if (shiftType == WorkShiftType.None)
                    {
                        contribution = 0.0m;
                    }
                    else if (shiftType == WorkShiftType.FullDay)
                    {
                        if (startDayPart == LeaveDayPart.Morning || startDayPart == LeaveDayPart.FullDay)
                        {
                            contribution = 1.0m;
                        }
                        else
                        {
                            contribution = 0.5m; // Afternoon
                        }
                    }
                    else if (shiftType == WorkShiftType.MorningOnly)
                    {
                        if (startDayPart == LeaveDayPart.Morning || startDayPart == LeaveDayPart.FullDay)
                        {
                            contribution = 0.5m;
                        }
                        else
                        {
                            return Result.Failure<decimal>(LeaveRequestErrors.InvalidShiftRegistration);
                        }
                    }
                    else if (shiftType == WorkShiftType.AfternoonOnly)
                    {
                        if (startDayPart == LeaveDayPart.Morning || startDayPart == LeaveDayPart.FullDay)
                        {
                            contribution = 0.5m;
                        }
                        else
                        {
                            contribution = 0.5m; // Afternoon
                        }
                    }
                }
                else if (currentDate == endDate)
                {
                    // Ngày kết thúc
                    if (shiftType == WorkShiftType.None)
                    {
                        contribution = 0.0m;
                    }
                    else if (shiftType == WorkShiftType.FullDay)
                    {
                        if (endDayPart == LeaveDayPart.Morning)
                        {
                            contribution = 0.5m;
                        }
                        else
                        {
                            contribution = 1.0m; // Afternoon hoặc FullDay
                        }
                    }
                    else if (shiftType == WorkShiftType.MorningOnly)
                    {
                        if (endDayPart == LeaveDayPart.Morning)
                        {
                            contribution = 0.5m;
                        }
                        else
                        {
                            contribution = 0.5m; // Afternoon hoặc FullDay
                        }
                    }
                    else if (shiftType == WorkShiftType.AfternoonOnly)
                    {
                        if (endDayPart == LeaveDayPart.Morning)
                        {
                            return Result.Failure<decimal>(LeaveRequestErrors.InvalidShiftRegistration);
                        }
                        else
                        {
                            contribution = 0.5m; // Afternoon hoặc FullDay
                        }
                    }
                }
                else
                {
                    // Ngày ở giữa (Middle dates)
                    if (shiftType == WorkShiftType.None)
                    {
                        contribution = 0.0m;
                    }
                    else if (shiftType == WorkShiftType.FullDay)
                    {
                        contribution = 1.0m;
                    }
                    else
                    {
                        contribution = 0.5m; // MorningOnly hoặc AfternoonOnly
                    }
                }
            }

            totalDuration += contribution;
            currentDate = currentDate.AddDays(1);
        }

        return Result.Success(totalDuration);
    }
}
