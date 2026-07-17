namespace Application.WorkCalendars.GetImportBatchDetails;

public sealed class CalendarImportBatchDetailsResponse
{
    public Guid BatchId { get; set; }
    public string Status { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public DateTime UploadedAt { get; set; }
    public List<BatchRowResponse> Rows { get; set; } = new();
}

public sealed class BatchRowResponse
{
    public Guid Id { get; set; }
    public int RowIndex { get; set; }
    public DateOnly? Date { get; set; }
    public string DayType { get; set; } = null!;
    public string WorkShift { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}
