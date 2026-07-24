using Domain.ApprovalRouting;
using Domain.LeaveRequests;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class ApprovalRouteAuditLogRepository : IApprovalRouteAuditLogRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ApprovalRouteAuditLogRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApprovalRouteAuditLog?> GetByIdAsync(ApprovalRouteAuditLogId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ApprovalRouteAuditLogs
            .Include(a => a.LeaveRequest)
            .Include(a => a.LeaveRequestApprovalAssignment)
            .Include(a => a.PreviousApprover)
            .Include(a => a.NewApprover)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<List<ApprovalRouteAuditLog>> GetByLeaveRequestIdAsync(LeaveRequestId leaveRequestId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ApprovalRouteAuditLogs
            .Include(a => a.LeaveRequestApprovalAssignment)
            .Include(a => a.PreviousApprover)
            .Include(a => a.NewApprover)
            .Where(a => a.LeaveRequestId == leaveRequestId)
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public void Add(ApprovalRouteAuditLog auditLog)
    {
        _dbContext.ApprovalRouteAuditLogs.Add(auditLog);
    }

    public IQueryable<ApprovalRouteAuditLog> GetEntitiesAsQueryable()
    {
        return _dbContext.ApprovalRouteAuditLogs.AsQueryable();
    }
}
