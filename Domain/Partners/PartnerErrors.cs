using Domain.Abstractions;

namespace Domain.Partners;

public class PartnerErrors
{
    public static Error NotFound = new(
        "Partner.Found",
        "The property with the specified identifier was not found");
}