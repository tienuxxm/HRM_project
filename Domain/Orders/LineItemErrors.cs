using Domain.Abstractions;

namespace Domain.Orders;

public class LineItemErrors
{
    public static Error NotFound = new(
        "LineItem.Found",
        "The LineItem with the specified identifier was not found");

  
}