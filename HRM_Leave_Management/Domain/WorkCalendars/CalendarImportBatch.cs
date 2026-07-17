using Domain.Abstractions;

namespace Domain.WorkCalendars;

public record CalendarImportBatchId(Guid Value)
{
    public static CalendarImportBatchId New() => new(Guid.NewGuid());
}

public enum ImportBatchStatus
{
    Draft = 1,
    Applied = 2,
    Failed = 3
}

public class CalendarImportBatch : Entity<CalendarImportBatchId>
{
    private readonly List<CalendarImportBatchRow> _rows = new();

    private CalendarImportBatch(
        CalendarImportBatchId id,
        string fileName,
        ImportBatchStatus status,
        Guid createdBy,
        DateTime createdAt)
    {
        Id = id;
        FileName = fileName;
        Status = status;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
    }

    private CalendarImportBatch()
    {
    }

    public string FileName { get; private set; } = null!;
    public ImportBatchStatus Status { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid? ProcessedBy { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public IReadOnlyCollection<CalendarImportBatchRow> Rows => _rows.AsReadOnly();

    public static CalendarImportBatch Create(string fileName, Guid createdBy)
    {
        return new CalendarImportBatch(
            CalendarImportBatchId.New(),
            fileName,
            ImportBatchStatus.Draft,
            createdBy,
            DateTime.UtcNow);
    }

    public void AddRow(CalendarImportBatchRow row)
    {
        _rows.Add(row);
    }

    public void MarkAsApplied(Guid processedBy, DateTime processedAt)
    {
        if (Status != ImportBatchStatus.Draft)
        {
            throw new InvalidOperationException("Only draft batches can be marked as applied.");
        }

        Status = ImportBatchStatus.Applied;
        ProcessedBy = processedBy;
        ProcessedAt = processedAt;
    }

    public void MarkAsFailed(Guid processedBy, DateTime processedAt)
    {
        if (Status != ImportBatchStatus.Draft)
        {
            throw new InvalidOperationException("Only draft batches can be marked as failed.");
        }

        Status = ImportBatchStatus.Failed;
        ProcessedBy = processedBy;
        ProcessedAt = processedAt;
    }
}
