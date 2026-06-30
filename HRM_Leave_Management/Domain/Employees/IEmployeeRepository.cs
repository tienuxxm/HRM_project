using System.Linq.Expressions;
using Domain.Abstractions;

namespace Domain.Employees;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(EmployeeId id, CancellationToken cancellationToken = default);

    void Add(Employee employee);

    void Update(Employee employee);

    void Remove(Employee employee);

    IQueryable<Employee> GetEntitiesAsQueryable();

    Task<PagedList<Employee>> GetAllPaged(PagedQuery<Employee, EmployeeId> request,
        IQueryable<Employee>? queryable = null);

    Task<bool> IsExistedAsync(Expression<Func<Employee, bool>> expression,
        CancellationToken cancellationToken = default);

    Task<List<Employee>?> GetAll(CancellationToken cancellationToken = default);
}
