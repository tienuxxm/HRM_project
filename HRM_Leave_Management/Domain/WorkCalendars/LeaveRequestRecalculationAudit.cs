using Domain.Abstractions;
using Domain.Employees;
using Domain.LeaveRequests;
using Domain.LeaveTypes;

namespace Domain.WorkCalendars;

public record LeaveRequestRecalculationAuditId(Guid Value)
{
    public static LeaveRequestRecalculationAuditId New() => new(Guid.NewGuid());
}

public enum RecalculationAuditStatus
{
    Success = 1,
    NeedsEmployeeRevision = 2,
    Failed = 3
}

public class LeaveRequestRecalculationAudit : Entity<LeaveRequestRecalculationAuditId>
{
    private LeaveRequestRecalculationAudit(
        LeaveRequestRecalculationAuditId id,
        CalendarImportBatchId? batchId,
        LeaveRequestId leaveRequestId,
        EmployeeId employeeId,
        LeaveTypeId leaveTypeId,
        LeaveRequestStatus oldStatus,
        LeaveRequestStatus newStatus,
        decimal oldDuration,
        decimal newDuration,
        Guid? oldProcessedBy,
        DateTime? oldProcessedAt,
        string? oldComment,
        DateTime recalculatedAt,
        RecalculationAuditStatus status,
        string? errorMessage)
    {
        Id = id;
        BatchId = batchId;
        LeaveRequestId = leaveRequestId;
        EmployeeId = employeeId;
        LeaveTypeId = leaveTypeId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        OldDuration = oldDuration;
        NewDuration = newDuration;
        OldProcessedBy = oldProcessedBy;
        OldProcessedAt = oldProcessedAt;
        OldComment = oldComment;
        RecalculatedAt = recalculatedAt;
        Status = status;
        ErrorMessage = errorMessage;
    }

    private LeaveRequestRecalculationAudit()
    {
    }

    public CalendarImportBatchId? BatchId { get; private set; }
    public CalendarImportBatch? Batch { get; private set; }
    public LeaveRequestId LeaveRequestId { get; private set; } = null!;
    public LeaveRequest LeaveRequest { get; private set; } = null!;
    public EmployeeId EmployeeId { get; private set; } = null!;
    public Employee Employee { get; private set; } = null!;
    public LeaveTypeId LeaveTypeId { get; private set; } = null!;
    public LeaveType LeaveType { get; private set; } = null!;
    public LeaveRequestStatus OldStatus { get; private set; }
    public LeaveRequestStatus NewStatus { get; private set; }
    public decimal OldDuration { get; private set; }
    public decimal NewDuration { get; private set; }
    public Guid? OldProcessedBy { get; private set; }
    public DateTime? OldProcessedAt { get; private set; }
    public string? OldComment { get; private set; }
    public DateTime RecalculatedAt { get; private set; }
    public RecalculationAuditStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }

    public static LeaveRequestRecalculationAudit Create(
        CalendarImportBatchId? batchId,
        LeaveRequest leaveRequest,
        LeaveRequestStatus oldStatus,
        LeaveRequestStatus newStatus,
        decimal oldDuration,
        decimal newDuration,
        RecalculationAuditStatus status,
        Guid? oldProcessedBy,
        DateTime? oldProcessedAt,
        string? oldComment,
        string? errorMessage = null)
    {
        return new LeaveRequestRecalculationAudit(
            LeaveRequestRecalculationAuditId.New(),
            batchId,
            leaveRequest.Id,
            leaveRequest.EmployeeId,
            leaveRequest.LeaveTypeId,
            oldStatus,
            newStatus,
            oldDuration,
            newDuration,
            oldProcessedBy,
            oldProcessedAt,
            oldComment,
            DateTime.UtcNow,
            status,
            errorMessage);
    }
}
