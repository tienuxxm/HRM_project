using System.Linq.Expressions;
using Domain.Abstractions;

namespace Domain.Departments;

public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(DepartmentId id, CancellationToken cancellationToken = default);

    void Add(Department department);

    void Update(Department department);

    void Remove(Department department);

    IQueryable<Department> GetEntitiesAsQueryable();

    Task<PagedList<Department>> GetAllPaged(PagedQuery<Department, DepartmentId> request,
        IQueryable<Department>? queryable = null);

    Task<bool> IsExistedAsync(Expression<Func<Department, bool>> expression,
        CancellationToken cancellationToken = default);

    Task<List<Department>?> GetAll(CancellationToken cancellationToken = default);
}
