namespace Domain.OrderFees;

public interface IOrderFeeRespository
{
    void Remove(OrderFee orderFee);
    void RemoveRange(List<OrderFee> orderFees);
}