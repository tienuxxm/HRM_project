using Domain.Departments;

namespace Infrastructure.Repositories
{
    internal sealed class DepartmentRepository : Repository<Department, DepartmentId>, IDepartmentRepository
    {
        public DepartmentRepository(ApplicationDbContext dbContext)
            : base(dbContext)
        {
        }
    }
}
