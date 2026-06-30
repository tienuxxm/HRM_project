namespace Domain.Orders;

public interface ILineItemRepository
{
    void RemoveRange(List<LineItem> invoices);
    void Remove(LineItem invoice);
}