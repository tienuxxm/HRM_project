using Domain.LeaveRequests;
using Domain.WorkCalendars;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class LeaveRequestRecalculationAuditRepository : Repository<LeaveRequestRecalculationAudit, LeaveRequestRecalculationAuditId>, ILeaveRequestRecalculationAuditRepository
{
    public LeaveRequestRecalculationAuditRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task AddAsync(LeaveRequestRecalculationAudit audit, CancellationToken cancellationToken = default)
    {
        await DbContext.Set<LeaveRequestRecalculationAudit>().AddAsync(audit, cancellationToken);
    }

    public async Task<List<LeaveRequestRecalculationAudit>> GetByBatchIdAsync(CalendarImportBatchId batchId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<LeaveRequestRecalculationAudit>()
            .Where(lrra => lrra.BatchId == batchId)
            .OrderBy(lrra => lrra.RecalculatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LeaveRequestRecalculationAudit>> GetByLeaveRequestIdAsync(LeaveRequestId leaveRequestId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<LeaveRequestRecalculationAudit>()
            .Where(lrra => lrra.LeaveRequestId == leaveRequestId)
            .OrderByDescending(lrra => lrra.RecalculatedAt)
            .ToListAsync(cancellationToken);
    }
}
