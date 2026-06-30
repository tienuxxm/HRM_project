using Domain.Employees;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    internal sealed class EmployeeRepository : Repository<Employee, EmployeeId>, IEmployeeRepository
    {
        public EmployeeRepository(ApplicationDbContext dbContext)
            : base(dbContext)
        {
        }

        public new async Task<Employee?> GetByIdAsync(EmployeeId id, CancellationToken cancellationToken = default)
        {
            return await DbContext.Set<Employee>()
                .Include(e => e.Position)
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public new async Task<List<Employee>?> GetAll(CancellationToken cancellationToken = default)
        {
            return await DbContext.Set<Employee>()
                .Include(e => e.Position)
                .Include(e => e.Department)
                .ToListAsync(cancellationToken);
        }

        public new IQueryable<Employee> GetEntitiesAsQueryable()
        {
            return DbContext.Set<Employee>()
                .Include(e => e.Position)
                .Include(e => e.Department)
                .AsQueryable();
        }
    }
}
