namespace Domain.Bookings;

public class BookingTime
{
    private BookingTime()
    {
    }
    public DayOfWeek DayOfWeek { get; init; }
    public DateOnly Date { get; init; }
    public TimeOnly Time { get; init; }
    
    public static BookingTime Create(DateTime dateTime)
    {
        return new BookingTime
        {
            Date = DateOnly.FromDateTime(dateTime),
            Time = TimeOnly.FromDateTime(dateTime),
            DayOfWeek = dateTime.DayOfWeek
        };
    }

    public DateTime GetDateTime()
    {
        return Date.ToDateTime(Time);
    }
}