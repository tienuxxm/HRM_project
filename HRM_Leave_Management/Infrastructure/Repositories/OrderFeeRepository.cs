using Domain.OrderFees;

namespace Infrastructure.Repositories;

internal sealed class OrderFeeRepository : Repository<OrderFee, OrderFeeId>, IOrderFeeRespository
{
    public OrderFeeRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}