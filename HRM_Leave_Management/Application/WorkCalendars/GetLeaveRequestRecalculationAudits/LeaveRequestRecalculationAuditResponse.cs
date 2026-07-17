namespace Application.WorkCalendars.GetLeaveRequestRecalculationAudits;

public sealed class LeaveRequestRecalculationAuditResponse
{
    public Guid Id { get; set; }
    public Guid? BatchId { get; set; }
    public string OldStatus { get; set; } = null!;
    public string NewStatus { get; set; } = null!;
    public decimal OldDuration { get; set; }
    public decimal NewDuration { get; set; }
    public string? OldProcessedBy { get; set; }
    public DateTime? OldProcessedAt { get; set; }
    public DateTime RecalculatedAt { get; set; }
}
