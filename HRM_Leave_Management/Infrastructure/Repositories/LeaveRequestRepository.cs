using Domain.LeaveRequests;

namespace Infrastructure.Repositories;

internal sealed class LeaveRequestRepository : Repository<LeaveRequest, LeaveRequestId>, ILeaveRequestRepository
{
    public LeaveRequestRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }
}
