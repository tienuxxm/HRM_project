using Domain.WorkCalendars;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class CalendarImportBatchRowRepository : Repository<CalendarImportBatchRow, CalendarImportBatchRowId>, ICalendarImportBatchRowRepository
{
    public CalendarImportBatchRowRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<List<CalendarImportBatchRow>> GetByBatchIdAsync(CalendarImportBatchId batchId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<CalendarImportBatchRow>()
            .Where(cibr => cibr.BatchId == batchId)
            .OrderBy(cibr => cibr.RowIndex)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CalendarImportBatchRow row, CancellationToken cancellationToken = default)
    {
        await DbContext.Set<CalendarImportBatchRow>().AddAsync(row, cancellationToken);
    }
}
