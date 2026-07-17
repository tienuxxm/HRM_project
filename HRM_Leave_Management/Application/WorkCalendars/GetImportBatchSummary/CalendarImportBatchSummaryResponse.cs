namespace Application.WorkCalendars.GetImportBatchSummary;

public sealed class CalendarImportBatchSummaryResponse
{
    public Guid BatchId { get; set; }
    public string FileName { get; set; } = null!;
    public DateTime UploadedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int UpdatedDaysCount { get; set; }
    public List<RecalculationAuditResponse> AffectedRequests { get; set; } = new();
}

public sealed class RecalculationAuditResponse
{
    public Guid AuditId { get; set; }
    public Guid LeaveRequestId { get; set; }
    public string EmployeeName { get; set; } = null!;
    public string LeaveTypeName { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string OldStatus { get; set; } = null!;
    public string NewStatus { get; set; } = null!;
    public decimal OldDuration { get; set; }
    public decimal NewDuration { get; set; }
}
