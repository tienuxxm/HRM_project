using Domain.Abstractions;

namespace Domain.WorkCalendars;

public record CalendarImportBatchRowId(Guid Value)
{
    public static CalendarImportBatchRowId New() => new(Guid.NewGuid());
}

public enum ImportRowStatus
{
    Draft = 1,
    Valid = 2,
    Invalid = 3,
    Applied = 4
}

public class CalendarImportBatchRow : Entity<CalendarImportBatchRowId>
{
    private CalendarImportBatchRow(
        CalendarImportBatchRowId id,
        CalendarImportBatchId batchId,
        int rowIndex,
        DateOnly? date,
        CalendarDayType? dayType,
        WorkShiftType? workShift,
        string? description,
        bool isActive,
        ImportRowStatus status,
        string? errorMessage,
        string? rawDate,
        string? rawDayType,
        string? rawWorkShift)
    {
        Id = id;
        BatchId = batchId;
        RowIndex = rowIndex;
        Date = date;
        DayType = dayType;
        WorkShift = workShift;
        Description = description;
        IsActive = isActive;
        Status = status;
        ErrorMessage = errorMessage;
        RawDate = rawDate;
        RawDayType = rawDayType;
        RawWorkShift = rawWorkShift;
    }

    private CalendarImportBatchRow()
    {
    }

    public CalendarImportBatchId BatchId { get; private set; } = null!;
    public CalendarImportBatch Batch { get; private set; } = null!;
    public int RowIndex { get; private set; }
    public DateOnly? Date { get; private set; }
    public CalendarDayType? DayType { get; private set; }
    public WorkShiftType? WorkShift { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public ImportRowStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? RawDate { get; private set; }
    public string? RawDayType { get; private set; }
    public string? RawWorkShift { get; private set; }

    public static CalendarImportBatchRow Create(
        CalendarImportBatchId batchId,
        int rowIndex,
        DateOnly? date,
        CalendarDayType? dayType,
        WorkShiftType? workShift,
        string? description,
        bool isActive,
        string? rawDate,
        string? rawDayType,
        string? rawWorkShift)
    {
        return new CalendarImportBatchRow(
            CalendarImportBatchRowId.New(),
            batchId,
            rowIndex,
            date,
            dayType,
            workShift,
            description,
            isActive,
            ImportRowStatus.Draft,
            errorMessage: null,
            rawDate,
            rawDayType,
            rawWorkShift);
    }

    public void MarkAsValid()
    {
        Status = ImportRowStatus.Valid;
        ErrorMessage = null;
    }

    public void MarkAsInvalid(string errorMessage)
    {
        Status = ImportRowStatus.Invalid;
        ErrorMessage = errorMessage;
    }

    public void MarkAsApplied()
    {
        Status = ImportRowStatus.Applied;
    }
}
