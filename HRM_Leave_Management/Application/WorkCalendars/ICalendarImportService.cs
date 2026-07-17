using Domain.Abstractions;
using Domain.WorkCalendars;

namespace Application.WorkCalendars;

public interface ICalendarImportService
{
    Task<Result<CalendarImportBatch>> ParseAndSaveDraftAsync(
        string fileName,
        Stream excelStream,
        Guid createdBy,
        CancellationToken cancellationToken = default);

    Task<Result<CalendarImportBatch>> ApplyBatchAsync(
        CalendarImportBatchId batchId,
        Guid processedBy,
        CancellationToken cancellationToken = default);
}
