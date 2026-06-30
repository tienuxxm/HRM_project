namespace Application.Abstractions.Clock;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateTime TimeStampToUtc(long timeStamp);

    DateTime ToVnTime(DateTime time);
}