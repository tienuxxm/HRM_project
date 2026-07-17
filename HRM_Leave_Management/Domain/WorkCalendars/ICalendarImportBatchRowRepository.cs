namespace Domain.WorkCalendars;

public interface ICalendarImportBatchRowRepository
{
    Task<CalendarImportBatchRow?> GetByIdAsync(CalendarImportBatchRowId id, CancellationToken cancellationToken = default);
    Task<List<CalendarImportBatchRow>> GetByBatchIdAsync(CalendarImportBatchId batchId, CancellationToken cancellationToken = default);
    Task AddAsync(CalendarImportBatchRow row, CancellationToken cancellationToken = default);
    void Update(CalendarImportBatchRow row);
}
