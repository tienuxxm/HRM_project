using Application.Abstractions.Messaging;
using Application.Abstractions.Clock;
using Domain.Abstractions;
using Domain.LeaveRequests;
using Domain.WorkCalendars;
using Microsoft.EntityFrameworkCore;

namespace Application.WorkCalendars.PreviewManualCalendarChange;

internal sealed class PreviewManualCalendarChangeQueryHandler : IQueryHandler<PreviewManualCalendarChangeQuery, List<AffectedLeaveRequestResponse>>
{
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IWorkCalendarDayRepository _workCalendarDayRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public PreviewManualCalendarChangeQueryHandler(
        ILeaveRequestRepository leaveRequestRepository,
        IWorkCalendarDayRepository workCalendarDayRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _leaveRequestRepository = leaveRequestRepository;
        _workCalendarDayRepository = workCalendarDayRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<List<AffectedLeaveRequestResponse>>> Handle(
        PreviewManualCalendarChangeQuery request,
        CancellationToken cancellationToken)
    {
        // 0. Validate not in the past
        var today = DateOnly.FromDateTime(_dateTimeProvider.ToVnTime(_dateTimeProvider.UtcNow));
        if (request.Date < today)
        {
            return Result.Failure<List<AffectedLeaveRequestResponse>>(WorkCalendarErrors.PastEditingNotAllowed);
        }

        var targetDate = request.Date;
        var year = targetDate.Year;

        // 1. Parse Enum values from input strings
        if (!Enum.TryParse<CalendarDayType>(request.DayType, true, out var inputDayType))
        {
            return Result.Failure<List<AffectedLeaveRequestResponse>>(new Error("Preview.InvalidDayType", $"Invalid DayType value '{request.DayType}'."));
        }

        if (!Enum.TryParse<WorkShiftType>(request.WorkShift, true, out var inputWorkShift))
        {
            return Result.Failure<List<AffectedLeaveRequestResponse>>(new Error("Preview.InvalidWorkShift", $"Invalid WorkShift value '{request.WorkShift}'."));
        }

        // Validate consistency rules
        if ((inputDayType == CalendarDayType.PublicHoliday || inputDayType == CalendarDayType.CompanyCustomNonWorkingDay)
            && inputWorkShift != WorkShiftType.None)
        {
            return Result.Failure<List<AffectedLeaveRequestResponse>>(new Error("Preview.InvalidCombination", $"DayType '{inputDayType}' must have WorkShift set to 'None'. Got '{inputWorkShift}'."));
        }
        else if ((inputDayType == CalendarDayType.WorkingSaturdayOverride || inputDayType == CalendarDayType.StandardWorkingDayOverride)
            && inputWorkShift == WorkShiftType.None)
        {
            return Result.Failure<List<AffectedLeaveRequestResponse>>(new Error("Preview.InvalidCombination", $"DayType '{inputDayType}' cannot have WorkShift set to 'None'."));
        }

        // 2. Fetch overlapping leave requests (Approved and Pending)
        var affectedRequests = await _leaveRequestRepository.GetEntitiesAsQueryable()
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .Where(lr => (lr.Status == LeaveRequestStatus.Approved || lr.Status == LeaveRequestStatus.Pending)
                         && lr.StartDate <= targetDate
                         && lr.EndDate >= targetDate)
            .ToListAsync(cancellationToken);

        if (!affectedRequests.Any())
        {
            return Result.Success(new List<AffectedLeaveRequestResponse>());
        }

        // 3. Build virtual work calendar config dict for the year
        var realActiveDays = await _workCalendarDayRepository.GetActiveByYearAsync(year, cancellationToken);
        
        var virtualConfig = realActiveDays
            .Where(d => d.Date != targetDate) // exclude real record of the target date if any
            .ToDictionary(
                d => d.Date,
                d => (d.DayType, d.WorkShift)
            );

        // If standard non-working day override (to reset standard day behavior)
        bool shouldAddVirtual = request.IsActive && request.DayType != "StandardNonWorkingDayOverride";

        if (shouldAddVirtual)
        {
            virtualConfig[targetDate] = (inputDayType, inputWorkShift);
        }

        // 4. Recalculate duration locally and filter really affected requests
        var results = new List<AffectedLeaveRequestResponse>();

        foreach (var lr in affectedRequests)
        {
            var calculateResult = CalculateLeaveDurationLocal(
                lr.StartDate,
                lr.EndDate,
                lr.StartDayPart,
                lr.EndDayPart,
                virtualConfig);

            if (calculateResult.IsSuccess)
            {
                var newDuration = calculateResult.Value;
                if (newDuration != lr.Duration)
                {
                    string oldStatusStr = lr.Status.ToString();
                    string newStatusStr = oldStatusStr;

                    if (lr.Status == LeaveRequestStatus.Approved)
                    {
                        newStatusStr = LeaveRequestStatus.Pending.ToString();
                    }

                    results.Add(new AffectedLeaveRequestResponse(
                        lr.Id.Value,
                        lr.Employee.FullName,
                        lr.LeaveType.Name,
                        lr.StartDate,
                        lr.EndDate,
                        lr.Duration,
                        newDuration,
                        oldStatusStr,
                        newStatusStr
                    ));
                }
            }
        }

        return Result.Success(results);
    }

    private Result<decimal> CalculateLeaveDurationLocal(
        DateOnly startDate,
        DateOnly endDate,
        LeaveDayPart startDayPart,
        LeaveDayPart endDayPart,
        Dictionary<DateOnly, (CalendarDayType DayType, WorkShiftType WorkShift)> configDict)
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
                        contribution = 0.5m;
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
                if (currentDate == startDate)
                {
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
                            contribution = 0.5m;
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
                            contribution = 0.5m;
                        }
                    }
                }
                else if (currentDate == endDate)
                {
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
                            contribution = 1.0m;
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
                            contribution = 0.5m;
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
                            contribution = 0.5m;
                        }
                    }
                }
                else
                {
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
                        contribution = 0.5m;
                    }
                }
            }

            totalDuration += contribution;
            currentDate = currentDate.AddDays(1);
        }

        return Result.Success(totalDuration);
    }
}
