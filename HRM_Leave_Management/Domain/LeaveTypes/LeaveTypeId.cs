namespace Domain.LeaveTypes;

public record LeaveTypeId(Guid Value)
{
    public static LeaveTypeId New() => new(Guid.NewGuid());
}
