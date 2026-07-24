using Domain.Abstractions;
using Domain.Employees;
using Domain.LeaveRequests;

namespace Domain.ApprovalRouting;

public class ApprovalRouteAuditLog : Entity<ApprovalRouteAuditLogId>
{
    private ApprovalRouteAuditLog(
        ApprovalRouteAuditLogId id,
        LeaveRequestId leaveRequestId,
        LeaveRequestApprovalAssignmentId? assignmentId,
        EmployeeId? previousApproverEmployeeId,
        EmployeeId? newApproverEmployeeId,
        ApprovalRouteAuditActionType actionType,
        string? oldAssignmentStatus,
        string newAssignmentStatus,
        string reasonCode,
        string? note,
        Guid createdByUserId,
        DateTime createdDate)
        : base(id)
    {
        LeaveRequestId = leaveRequestId;
        LeaveRequestApprovalAssignmentId = assignmentId;
        PreviousApproverEmployeeId = previousApproverEmployeeId;
        NewApproverEmployeeId = newApproverEmployeeId;
        ActionType = actionType;
        OldAssignmentStatus = oldAssignmentStatus;
        NewAssignmentStatus = newAssignmentStatus;
        ReasonCode = reasonCode;
        Note = note;
        CreatedByUserId = createdByUserId;
        CreatedDate = createdDate;
    }

    private ApprovalRouteAuditLog()
    {
    }

    public LeaveRequestId LeaveRequestId { get; private set; } = null!;
    public LeaveRequest? LeaveRequest { get; private set; }
    public LeaveRequestApprovalAssignmentId? LeaveRequestApprovalAssignmentId { get; private set; }
    public LeaveRequestApprovalAssignment? LeaveRequestApprovalAssignment { get; private set; }
    public EmployeeId? PreviousApproverEmployeeId { get; private set; }
    public Employee? PreviousApprover { get; private set; }
    public EmployeeId? NewApproverEmployeeId { get; private set; }
    public Employee? NewApprover { get; private set; }
    public ApprovalRouteAuditActionType ActionType { get; private set; }
    public string? OldAssignmentStatus { get; private set; }
    public string NewAssignmentStatus { get; private set; } = null!;
    public string ReasonCode { get; private set; } = null!;
    public string? Note { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedDate { get; private set; }

    public static ApprovalRouteAuditLog LogAction(
        LeaveRequestId leaveRequestId,
        LeaveRequestApprovalAssignmentId? assignmentId,
        EmployeeId? previousApproverId,
        EmployeeId? newApproverId,
        ApprovalRouteAuditActionType actionType,
        string? oldStatus,
        string newStatus,
        string reasonCode,
        Guid createdByUserId,
        string? note = null)
    {
        if (leaveRequestId == null)
            throw new ArgumentNullException(nameof(leaveRequestId));

        if (string.IsNullOrWhiteSpace(newStatus))
            throw new ArgumentException("New assignment status is required.", nameof(newStatus));

        if (string.IsNullOrWhiteSpace(reasonCode))
            throw new ArgumentException("Reason code is required.", nameof(reasonCode));

        return new ApprovalRouteAuditLog(
            ApprovalRouteAuditLogId.New(),
            leaveRequestId,
            assignmentId,
            previousApproverId,
            newApproverId,
            actionType,
            oldStatus,
            newStatus,
            reasonCode,
            note,
            createdByUserId,
            createdDate: DateTime.UtcNow);
    }
}
