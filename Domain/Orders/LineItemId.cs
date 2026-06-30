namespace Domain.Orders;

public record LineItemId(Guid Value)
{ public static LineItemId New() => new(Guid.NewGuid());
}