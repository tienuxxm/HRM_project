namespace Domain.LeaveBalances;

public record LeaveBalanceId(Guid Value)
{
    public static LeaveBalanceId New() => new(Guid.NewGuid());
}
