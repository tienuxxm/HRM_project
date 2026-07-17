using Domain.WorkCalendars;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class WorkCalendarDayRepository : Repository<WorkCalendarDay, WorkCalendarDayId>, IWorkCalendarDayRepository
{
    public WorkCalendarDayRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<WorkCalendarDay?> GetActiveByDateAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<WorkCalendarDay>()
            .FirstOrDefaultAsync(wcd => wcd.IsActive && wcd.Date == date, cancellationToken);
    }

    public async Task<WorkCalendarDay?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<WorkCalendarDay>()
            .FirstOrDefaultAsync(wcd => wcd.Date == date, cancellationToken);
    }

    public async Task<List<WorkCalendarDay>> GetActiveByYearAsync(int year, CancellationToken cancellationToken = default)
    {
        var start = new DateOnly(year, 1, 1);
        var end = new DateOnly(year, 12, 31);

        return await DbContext.Set<WorkCalendarDay>()
            .Where(wcd => wcd.IsActive && wcd.Date >= start && wcd.Date <= end)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(WorkCalendarDay day, CancellationToken cancellationToken = default)
    {
        await DbContext.Set<WorkCalendarDay>().AddAsync(day, cancellationToken);
    }
}
