using Domain.Restaurants;
using Domain.Shared;

namespace Domain.Orders;

public class PricingService
{
    public PricingDetails CalculatePrice(Order order, Restaurant restaurant)
    {
        var priceProduct = new Money(order.LineItems.Sum(x => x.Price.Amount), Currency.Vnd);
        var serviceFee = new Money(priceProduct.Amount * 10 / 100, Currency.Vnd);
        var totalPrice = priceProduct + serviceFee;
        return new PricingDetails(priceProduct, serviceFee,  totalPrice);
    }
}