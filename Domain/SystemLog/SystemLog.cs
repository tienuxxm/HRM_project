using Domain.Abstractions;

namespace Domain.SystemLog;

public class SystemLog : Entity<SystemLogId>
{
    public SystemLog()
    {
    }

    private SystemLog(SystemLogId id, string message) : base(id)
    {
        Message = message;
    }

    public string Message { get; private set; }

    public static SystemLog Create(string message)
    {
        var log = new SystemLog(SystemLogId.New, message);
        return log;
    }
}