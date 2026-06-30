namespace Domain.FreeServices;

public record FeeServiceId(Guid Value)
{
    public static FeeServiceId New => new FeeServiceId(Guid.NewGuid());
}