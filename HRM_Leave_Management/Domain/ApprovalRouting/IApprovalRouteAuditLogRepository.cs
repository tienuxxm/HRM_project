using Domain.LeaveRequests;

namespace Domain.ApprovalRouting;

public interface IApprovalRouteAuditLogRepository
{
    Task<ApprovalRouteAuditLog?> GetByIdAsync(ApprovalRouteAuditLogId id, CancellationToken cancellationToken = default);
    Task<List<ApprovalRouteAuditLog>> GetByLeaveRequestIdAsync(LeaveRequestId leaveRequestId, CancellationToken cancellationToken = default);
    void Add(ApprovalRouteAuditLog auditLog);
    IQueryable<ApprovalRouteAuditLog> GetEntitiesAsQueryable();
}
