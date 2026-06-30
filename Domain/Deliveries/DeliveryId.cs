namespace Domain.Deliveries;

public record DeliveryId(Guid Value)
{
    public static DeliveryId New() => new(Guid.NewGuid());
}