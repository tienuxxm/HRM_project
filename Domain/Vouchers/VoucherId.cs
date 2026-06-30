namespace Domain.Vouchers;

public record VoucherId(Guid Value)
{
    public static VoucherId New() => new(Guid.NewGuid());
}