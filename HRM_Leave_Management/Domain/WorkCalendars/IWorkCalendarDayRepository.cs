namespace Domain.WorkCalendars;

public interface IWorkCalendarDayRepository
{
    Task<WorkCalendarDay?> GetByIdAsync(WorkCalendarDayId id, CancellationToken cancellationToken = default);
    Task<WorkCalendarDay?> GetActiveByDateAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<WorkCalendarDay?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<List<WorkCalendarDay>> GetActiveByYearAsync(int year, CancellationToken cancellationToken = default);
    Task AddAsync(WorkCalendarDay day, CancellationToken cancellationToken = default);
    void Update(WorkCalendarDay day);
}
