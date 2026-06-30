/*
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Dapper;
using Domain.Abstractions;

namespace Application.Orders.GetOrder;

internal sealed class GetOrderQueryHandler : IQueryHandler<GetOrderQuery, OrderResponse>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    public GetOrderQueryHandler(
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<OrderResponse>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        
        const string sql =  "SELECT o.id AS OrderId, i.* FROM order o " +
                            "LEFT JOIN line_item e ON o.id = e.order_id";
    
        var orderDictionary = new Dictionary<Guid, OrderResponse>(); // Used to store unique departments
        var result = connection.Query<OrderResponse, LineItemResponse, OrderResponse>(
            sql,
            (order, lineItem) =>
            {
                if (!orderDictionary.TryGetValue(order.Id, out var orderEntry))
                {
                    orderEntry = order;
                    order.LineItems = new List<LineItemResponse>();
                    orderDictionary.Add(orderEntry.Id, orderEntry);
                }
                if (lineItem != null)
                {
                    orderEntry.LineItems.Add(lineItem);
                }
            
                return orderEntry;
            },
            splitOn: "OrderId"
        );
        return result.FirstOrDefault();
       
    }
}
*/

