using Domain.Shared;

namespace Domain.Orders;

public record PricingDetails(
    Money ProductPrice,
    Money ServiceFee,
    Money TotalPrice);