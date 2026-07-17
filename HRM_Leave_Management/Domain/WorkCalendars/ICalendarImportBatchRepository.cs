namespace Domain.WorkCalendars;

public interface ICalendarImportBatchRepository
{
    Task<CalendarImportBatch?> GetByIdAsync(CalendarImportBatchId id, CancellationToken cancellationToken = default);
    Task AddAsync(CalendarImportBatch batch, CancellationToken cancellationToken = default);
    void Update(CalendarImportBatch batch);
}
