using Domain.ApprovalRouting;
using Domain.Departments;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class ApprovalRoutePolicyRepository : Repository<ApprovalRoutePolicy, ApprovalRoutePolicyId>, IApprovalRoutePolicyRepository
{
    public ApprovalRoutePolicyRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<ApprovalRoutePolicy?> GetByDepartmentIdAsync(DepartmentId departmentId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<ApprovalRoutePolicy>()
            .Include(p => p.Levels.Where(l => l.IsActive))
                .ThenInclude(l => l.Assignments.Where(a => a.IsActive))
            .Include(p => p.Rules.Where(r => r.IsActive))
                .ThenInclude(r => r.Candidates.Where(c => c.IsActive))
            .FirstOrDefaultAsync(p => p.DepartmentId == departmentId && p.IsActive, cancellationToken);
    }

    public async Task<bool> HasActivePolicyForDepartmentAsync(DepartmentId departmentId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<ApprovalRoutePolicy>()
            .AnyAsync(p => p.DepartmentId == departmentId && p.IsActive, cancellationToken);
    }
}
