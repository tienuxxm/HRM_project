namespace Domain.SystemLog;

public interface ISystemLogRepository
{
    void Add(SystemLog systemLog);
}