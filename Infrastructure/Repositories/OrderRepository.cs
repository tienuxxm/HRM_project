using Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class OrderRepository : Repository<Order, OrderId>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Order?> GetOrderIncludeLineItemByIdAsync(OrderId id, CancellationToken cancellationToken)
    {
        return await DbContext.Set<Order>()
            .Include(x => x.LineItems)
            .Include(x => x.Delivery)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}