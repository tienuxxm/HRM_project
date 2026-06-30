namespace Domain.Positions;

public record PositionId(Guid Value)
{
    public static PositionId New() => new(Guid.NewGuid());
}
