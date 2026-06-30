namespace Domain.MemberPointHistories;

public record MemberPointHistoryId(Guid Value)
{
    public static MemberPointHistoryId New() => new(Guid.NewGuid());
}