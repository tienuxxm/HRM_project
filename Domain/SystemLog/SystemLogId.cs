namespace Domain.SystemLog;

public record SystemLogId(Guid Value)
{
    public static SystemLogId New => new(Guid.NewGuid());
}