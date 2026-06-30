namespace Domain.Members;

public record MemberId(Guid Value)
{
    public static MemberId New() => new(Guid.NewGuid());
}