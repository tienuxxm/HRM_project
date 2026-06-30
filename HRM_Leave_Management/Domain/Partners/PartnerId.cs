namespace Domain.Partners;

public record PartnerId(Guid Value)
{
    public static PartnerId New() => new(Guid.NewGuid());
}