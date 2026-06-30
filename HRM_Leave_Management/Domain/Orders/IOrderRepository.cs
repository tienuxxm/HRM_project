using System.Linq.Expressions;
using Domain.Abstractions;

namespace Domain.Orders;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default);

    void Add(Order order);
    void Update(Order order);
    void UpdateRange(List<Order> orders);
    void Remove(Order order);

    Task<Order?> GetOrderIncludeLineItemByIdAsync(OrderId id, CancellationToken cancellationToken = default);
    IQueryable<Order> GetEntitiesAsQueryable();

    Task<Order?> GetLatestByProperty(Expression<Func<Order, dynamic>> expression,
        CancellationToken cancellationToken = default);

    Task<PagedList<Order>> GetAllPaged(PagedQuery<Order, OrderId> request,
        IQueryable<Order>? queryable = null);
}