using Domain.Abstractions;

namespace Domain.Promotions;

public class PromotionErrors
{
    public static Error NotFound = new(
        "Promotion.Found",
        "The property with the specified identifier was not found");
}