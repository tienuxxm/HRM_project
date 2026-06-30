using Domain.Abstractions;
using Domain.Orders;
using Domain.Shared;

namespace Domain.OrderFees;

public class OrderFee : Entity<OrderFeeId>
{
    public OrderId OrderId { get; private set; }
    public OrderFeeName OrderFeeName { get; private set; }
    public OrderFeeValue OrderFeeValue { get; private set; }
    public Money OrderFeeCharge { get; private set; }
    public bool IsPercent { get; private set; }

    private OrderFee()
    {
    }

    private OrderFee(OrderFeeId id, OrderId orderId, OrderFeeName orderFeeName, OrderFeeValue orderFeeValue,
        Money orderFeeCharge, bool isPercent) : base(id)
    {
        OrderId = orderId;
        OrderFeeName = orderFeeName;
        OrderFeeValue = orderFeeValue;
        OrderFeeCharge = orderFeeCharge;
        IsPercent = isPercent;
    }

    public static OrderFee Create(OrderId orderId, OrderFeeName orderFeeName, OrderFeeValue orderFeeValue,
        Money orderFeeCharge, bool isPercent)
    {
        return new OrderFee(OrderFeeId.New, orderId, orderFeeName, orderFeeValue, orderFeeCharge, isPercent);
    }
}