using Domain.LeaveApproverAssignments;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories;

internal sealed class LeaveApproverAssignmentRepository : Repository<LeaveApproverAssignment, LeaveApproverAssignmentId>, ILeaveApproverAssignmentRepository
{
    public LeaveApproverAssignmentRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public new async Task<LeaveApproverAssignment?> GetByIdAsync(LeaveApproverAssignmentId id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<LeaveApproverAssignment>()
            .Include(a => a.Approver)
            .Include(a => a.TargetDepartment)
            .Include(a => a.TargetPosition)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public new async Task<List<LeaveApproverAssignment>?> GetAll(CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<LeaveApproverAssignment>()
            .Include(a => a.Approver)
            .Include(a => a.TargetDepartment)
            .Include(a => a.TargetPosition)
            .ToListAsync(cancellationToken);
    }

    public new IQueryable<LeaveApproverAssignment> GetEntitiesAsQueryable()
    {
        return DbContext.Set<LeaveApproverAssignment>()
            .Include(a => a.Approver)
            .Include(a => a.TargetDepartment)
            .Include(a => a.TargetPosition)
            .AsQueryable();
    }
}
