namespace Domain.Deliveries;

public interface IDeliveryRepository
{
    Task<Delivery?> GetByIdAsync(DeliveryId id, CancellationToken cancellationToken = default);

    void Add(Delivery delivery);
    void RemoveRange(List<Delivery> invoices);
    void Remove(Delivery invoice);
}