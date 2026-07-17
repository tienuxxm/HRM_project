using Domain.WorkCalendars;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class CalendarImportBatchRepository : Repository<CalendarImportBatch, CalendarImportBatchId>, ICalendarImportBatchRepository
{
    public CalendarImportBatchRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public new async Task<CalendarImportBatch?> GetByIdAsync(CalendarImportBatchId id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<CalendarImportBatch>()
            .Include(cib => cib.Rows)
            .FirstOrDefaultAsync(cib => cib.Id == id, cancellationToken);
    }

    public async Task AddAsync(CalendarImportBatch batch, CancellationToken cancellationToken = default)
    {
        await DbContext.Set<CalendarImportBatch>().AddAsync(batch, cancellationToken);
    }
}
