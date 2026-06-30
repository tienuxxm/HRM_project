using Domain.LeaveTypes;

namespace Infrastructure.Repositories
{
    internal sealed class LeaveTypeRepository : Repository<LeaveType, LeaveTypeId>, ILeaveTypeRepository
    {
        public LeaveTypeRepository(ApplicationDbContext dbContext)
            : base(dbContext)
        {
        }
    }
}
