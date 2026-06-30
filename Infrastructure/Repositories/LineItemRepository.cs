using Domain.Orders;

namespace Infrastructure.Repositories;

internal sealed class LineItemRepository : Repository<LineItem, LineItemId>, ILineItemRepository
{
    public LineItemRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}