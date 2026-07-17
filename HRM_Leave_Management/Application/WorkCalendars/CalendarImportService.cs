using Application.Abstractions.Clock;
using Domain.Abstractions;
using Domain.LeaveRequests;
using Domain.WorkCalendars;
using Domain.LeaveBalances;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Drawing;

namespace Application.WorkCalendars;

public sealed class CalendarImportService : ICalendarImportService
{
    private readonly ICalendarImportBatchRepository _calendarImportBatchRepository;
    private readonly ICalendarImportBatchRowRepository _calendarImportBatchRowRepository;
    private readonly IWorkCalendarDayRepository _workCalendarDayRepository;
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly ILeaveRequestRecalculationAuditRepository _leaveRequestRecalculationAuditRepository;
    private readonly ILeaveBalanceRepository _leaveBalanceRepository;
    private readonly IWorkCalendarService _workCalendarService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public CalendarImportService(
        ICalendarImportBatchRepository calendarImportBatchRepository,
        ICalendarImportBatchRowRepository calendarImportBatchRowRepository,
        IWorkCalendarDayRepository workCalendarDayRepository,
        ILeaveRequestRepository leaveRequestRepository,
        ILeaveRequestRecalculationAuditRepository leaveRequestRecalculationAuditRepository,
        ILeaveBalanceRepository leaveBalanceRepository,
        IWorkCalendarService workCalendarService,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _calendarImportBatchRepository = calendarImportBatchRepository;
        _calendarImportBatchRowRepository = calendarImportBatchRowRepository;
        _workCalendarDayRepository = workCalendarDayRepository;
        _leaveRequestRepository = leaveRequestRepository;
        _leaveRequestRecalculationAuditRepository = leaveRequestRecalculationAuditRepository;
        _leaveBalanceRepository = leaveBalanceRepository;
        _workCalendarService = workCalendarService;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CalendarImportBatch>> ParseAndSaveDraftAsync(
        string fileName,
        Stream excelStream,
        Guid createdBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(excelStream);
            
            if (package.Workbook.Worksheets.Count == 0)
            {
                return Result.Failure<CalendarImportBatch>(new Error("Excel.EmptyWorkbook", "Excel workbook contains no worksheets."));
            }

            var worksheet = package.Workbook.Worksheets[0];
            if (worksheet.Dimension == null)
            {
                return Result.Failure<CalendarImportBatch>(new Error("Excel.EmptyWorksheet", "The first worksheet is empty."));
            }

            var batch = CalendarImportBatch.Create(fileName, createdBy);
            await _calendarImportBatchRepository.AddAsync(batch, cancellationToken);

            int colDate = 1, colDayType = 2, colWorkShift = 3, colDesc = 4, colActive = 5;
            
            // Map columns based on headers if found
            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
                var headerVal = worksheet.Cells[1, col].Value?.ToString()?.Trim();
                if (string.Equals(headerVal, "Date", StringComparison.OrdinalIgnoreCase)) colDate = col;
                else if (string.Equals(headerVal, "DayType", StringComparison.OrdinalIgnoreCase) || string.Equals(headerVal, "Day Type", StringComparison.OrdinalIgnoreCase)) colDayType = col;
                else if (string.Equals(headerVal, "WorkShift", StringComparison.OrdinalIgnoreCase) || string.Equals(headerVal, "Work Shift", StringComparison.OrdinalIgnoreCase)) colWorkShift = col;
                else if (string.Equals(headerVal, "Description", StringComparison.OrdinalIgnoreCase)) colDesc = col;
                else if (string.Equals(headerVal, "IsActive", StringComparison.OrdinalIgnoreCase) || string.Equals(headerVal, "Is Active", StringComparison.OrdinalIgnoreCase)) colActive = col;
            }

            for (int rowNum = 2; rowNum <= worksheet.Dimension.End.Row; rowNum++)
            {
                bool isRowEmpty = true;
                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    if (worksheet.Cells[rowNum, col].Value != null)
                    {
                        isRowEmpty = false;
                        break;
                    }
                }
                if (isRowEmpty) continue;

                var rawDateVal = worksheet.Cells[rowNum, colDate].Value;
                var rawDayTypeVal = worksheet.Cells[rowNum, colDayType].Value;
                var rawWorkShiftVal = worksheet.Cells[rowNum, colWorkShift].Value;
                var rawDescVal = worksheet.Cells[rowNum, colDesc].Value;
                var rawActiveVal = worksheet.Cells[rowNum, colActive].Value;

                string? rawDate = rawDateVal?.ToString()?.Trim();
                string? rawDayType = rawDayTypeVal?.ToString()?.Trim();
                string? rawWorkShift = rawWorkShiftVal?.ToString()?.Trim();
                string? rawDesc = rawDescVal?.ToString()?.Trim();
                string? rawActive = rawActiveVal?.ToString()?.Trim();

                DateOnly? date = null;
                CalendarDayType? dayType = null;
                WorkShiftType? workShift = null;
                bool isActive = true;
                var errors = new List<string>();

                // Parse Date
                if (rawDateVal is DateTime dt)
                {
                    date = DateOnly.FromDateTime(dt);
                }
                else if (rawDateVal is double d)
                {
                    try
                    {
                        date = DateOnly.FromDateTime(DateTime.FromOADate(d));
                    }
                    catch
                    {
                        errors.Add($"Invalid date numeric value '{rawDate}'.");
                    }
                }
                else if (!string.IsNullOrWhiteSpace(rawDate))
                {
                    if (DateOnly.TryParse(rawDate, out var parsedDate))
                    {
                        date = parsedDate;
                    }
                    else if (DateTime.TryParse(rawDate, out var parsedDt))
                    {
                        date = DateOnly.FromDateTime(parsedDt);
                    }
                    else
                    {
                        errors.Add($"Cannot parse date '{rawDate}'.");
                    }
                }
                else
                {
                    errors.Add("Date is required.");
                }

                // Parse DayType
                if (!string.IsNullOrWhiteSpace(rawDayType))
                {
                    if (Enum.TryParse<CalendarDayType>(rawDayType, true, out var parsedDayType))
                    {
                        dayType = parsedDayType;
                    }
                    else
                    {
                        errors.Add($"Invalid DayType value '{rawDayType}'.");
                    }
                }
                else
                {
                    errors.Add("DayType is required.");
                }

                // Parse WorkShift
                if (!string.IsNullOrWhiteSpace(rawWorkShift))
                {
                    if (Enum.TryParse<WorkShiftType>(rawWorkShift, true, out var parsedWorkShift))
                    {
                        workShift = parsedWorkShift;
                    }
                    else
                    {
                        errors.Add($"Invalid WorkShift value '{rawWorkShift}'.");
                    }
                }
                else
                {
                    errors.Add("WorkShift is required.");
                }

                // Parse IsActive
                if (!string.IsNullOrWhiteSpace(rawActive))
                {
                    if (bool.TryParse(rawActive, out var parsedActive))
                    {
                        isActive = parsedActive;
                    }
                    else if (rawActive == "1")
                    {
                        isActive = true;
                    }
                    else if (rawActive == "0")
                    {
                        isActive = false;
                    }
                    else if (string.Equals(rawActive, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        isActive = true;
                    }
                    else if (string.Equals(rawActive, "false", StringComparison.OrdinalIgnoreCase))
                    {
                        isActive = false;
                    }
                    else
                    {
                        errors.Add($"Invalid IsActive value '{rawActive}'.");
                    }
                }

                // Validate not in the past
                if (date != null)
                {
                    var today = DateOnly.FromDateTime(_dateTimeProvider.ToVnTime(_dateTimeProvider.UtcNow));
                    if (date.Value < today)
                    {
                        errors.Add(WorkCalendarErrors.PastEditingNotAllowed.Name);
                    }
                }

                // Validate DayType/WorkShift consistency
                if (dayType != null && workShift != null)
                {
                    if ((dayType == CalendarDayType.PublicHoliday || dayType == CalendarDayType.CompanyCustomNonWorkingDay)
                        && workShift != WorkShiftType.None)
                    {
                        errors.Add($"DayType '{dayType}' must have WorkShift set to 'None'. Got '{workShift}'.");
                    }
                    else if ((dayType == CalendarDayType.WorkingSaturdayOverride || dayType == CalendarDayType.StandardWorkingDayOverride)
                        && workShift == WorkShiftType.None)
                    {
                        errors.Add($"DayType '{dayType}' cannot have WorkShift set to 'None'.");
                    }
                }

                var rowEntity = CalendarImportBatchRow.Create(
                    batch.Id,
                    rowNum,
                    date,
                    dayType,
                    workShift,
                    rawDesc,
                    isActive,
                    rawDate,
                    rawDayType,
                    rawWorkShift);

                if (errors.Count > 0)
                {
                    rowEntity.MarkAsInvalid(string.Join(" ", errors));
                }
                else
                {
                    rowEntity.MarkAsValid();
                }

                batch.AddRow(rowEntity);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(batch);
        }
        catch (Exception ex)
        {
            return Result.Failure<CalendarImportBatch>(new Error("Excel.ImportFailed", $"Failed to parse Excel file: {ex.Message}"));
        }
    }

    public async Task<Result<CalendarImportBatch>> ApplyBatchAsync(
        CalendarImportBatchId batchId,
        Guid processedBy,
        CancellationToken cancellationToken = default)
    {
        var batch = await _calendarImportBatchRepository.GetByIdAsync(batchId, cancellationToken);
        if (batch == null)
        {
            return Result.Failure<CalendarImportBatch>(new Error("ImportBatch.NotFound", "Import batch not found."));
        }

        if (batch.Status != ImportBatchStatus.Draft)
        {
            return Result.Failure<CalendarImportBatch>(new Error("ImportBatch.InvalidStatus", "Only draft batches can be applied."));
        }

        if (batch.Rows.Any(r => r.Status == ImportRowStatus.Invalid))
        {
            batch.MarkAsFailed(processedBy, DateTime.UtcNow);
            _calendarImportBatchRepository.Update(batch);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<CalendarImportBatch>(new Error("ImportBatch.ContainsErrors", "Cannot apply batch containing invalid rows."));
        }

        using var transaction = _unitOfWork.BeginTransaction();
        try
        {
            var changedDates = new List<DateOnly>();

            // 1. Update or create WorkCalendarDays
            foreach (var row in batch.Rows)
            {
                if (row.Date == null || row.DayType == null || row.WorkShift == null) continue;

                var existingDay = await _workCalendarDayRepository.GetByDateAsync(row.Date.Value, cancellationToken);
                if (existingDay != null)
                {
                    existingDay.Update(row.DayType.Value, row.WorkShift.Value, row.Description);
                    existingDay.SetActive(row.IsActive);
                    _workCalendarDayRepository.Update(existingDay);
                }
                else
                {
                    var newDay = WorkCalendarDay.Create(
                        row.Date.Value,
                        row.DayType.Value,
                        row.WorkShift.Value,
                        row.Description,
                        processedBy);
                    newDay.SetActive(row.IsActive);
                    await _workCalendarDayRepository.AddAsync(newDay, cancellationToken);
                }

                row.MarkAsApplied();
                changedDates.Add(row.Date.Value);
            }

            // SaveChanges to DB context so recalculation service sees updated calendar days
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 2. Recalculate affected leave requests
            if (changedDates.Count > 0)
            {
                var minDate = changedDates.Min();
                var maxDate = changedDates.Max();

                var affectedLeaveRequests = await _leaveRequestRepository.GetEntitiesAsQueryable()
                    .Where(lr => (lr.Status == LeaveRequestStatus.Approved || lr.Status == LeaveRequestStatus.Pending)
                                 && lr.StartDate <= maxDate
                                 && lr.EndDate >= minDate)
                    .ToListAsync(cancellationToken);

                var reallyAffected = affectedLeaveRequests
                    .Where(lr => changedDates.Any(d => d >= lr.StartDate && d <= lr.EndDate))
                    .ToList();

                foreach (var lr in reallyAffected)
                {
                    var oldStatus = lr.Status;
                    var oldDuration = lr.Duration;
                    var oldProcessedBy = lr.ProcessedBy;
                    var oldProcessedAt = lr.ProcessedAt;
                    var oldComment = lr.Comment;

                    var durationResult = await _workCalendarService.CalculateLeaveDurationAsync(
                        lr.StartDate,
                        lr.EndDate,
                        lr.StartDayPart,
                        lr.EndDayPart,
                        cancellationToken);

                    if (durationResult.IsSuccess)
                    {
                        var newDuration = durationResult.Value;

                        if (newDuration != oldDuration)
                        {
                            if (oldStatus == LeaveRequestStatus.Approved)
                            {
                                var leaveBalance = await _leaveBalanceRepository.GetEntitiesAsQueryable()
                                    .FirstOrDefaultAsync(lb => lb.EmployeeId == lr.EmployeeId 
                                                               && lb.LeaveTypeId == lr.LeaveTypeId 
                                                               && lb.Year == lr.StartDate.Year, 
                                                         cancellationToken);
                                if (leaveBalance == null)
                                {
                                    throw new InvalidOperationException($"Leave balance not found for employee {lr.EmployeeId}, leave type {lr.LeaveTypeId}, year {lr.StartDate.Year}");
                                }

                                leaveBalance.ReturnUsedDays(oldDuration);
                                _leaveBalanceRepository.Update(leaveBalance);

                                lr.ReopenToPending(newDuration);
                            }
                            else if (oldStatus == LeaveRequestStatus.Pending)
                            {
                                lr.UpdateDurationOnly(newDuration);
                            }

                            _leaveRequestRepository.Update(lr);

                            var auditStatus = newDuration == 0.0m
                                ? RecalculationAuditStatus.NeedsEmployeeRevision
                                : RecalculationAuditStatus.Success;

                            var errorMessage = newDuration == 0.0m
                                ? "New duration is 0.0. Needs employee revision or cancellation."
                                : null;

                            var audit = LeaveRequestRecalculationAudit.Create(
                                batch.Id,
                                lr,
                                oldStatus,
                                lr.Status,
                                oldDuration,
                                newDuration,
                                auditStatus,
                                oldProcessedBy,
                                oldProcessedAt,
                                oldComment,
                                errorMessage);

                            await _leaveRequestRecalculationAuditRepository.AddAsync(audit, cancellationToken);
                        }
                    }
                    else
                    {
                        var audit = LeaveRequestRecalculationAudit.Create(
                            batch.Id,
                            lr,
                            oldStatus,
                            oldStatus,
                            oldDuration,
                            oldDuration,
                            RecalculationAuditStatus.Failed,
                            oldProcessedBy,
                            oldProcessedAt,
                            oldComment,
                            durationResult.Error.Name);

                        await _leaveRequestRecalculationAuditRepository.AddAsync(audit, cancellationToken);
                    }
                }
            }

            batch.MarkAsApplied(processedBy, DateTime.UtcNow);
            _calendarImportBatchRepository.Update(batch);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            transaction.Commit();

            return Result.Success(batch);
        }
        catch (Exception ex)
        {
            try
            {
                transaction.Rollback();
            }
            catch
            {
                // Ignore rollback exception
            }

            try
            {
                batch.MarkAsFailed(processedBy, DateTime.UtcNow);
                _calendarImportBatchRepository.Update(batch);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                // Ignore secondary save failures during error handling
            }

            return Result.Failure<CalendarImportBatch>(new Error("ImportBatch.ApplyFailed", $"Failed to apply batch: {ex.Message}"));
        }
    }
}
