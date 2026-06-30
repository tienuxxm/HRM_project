using Domain.LeaveBalances;

namespace Infrastructure.Repositories
{
    internal sealed class LeaveBalanceRepository : Repository<LeaveBalance, LeaveBalanceId>, ILeaveBalanceRepository
    {
        public LeaveBalanceRepository(ApplicationDbContext dbContext)
            : base(dbContext)
        {
        }
    }
}
