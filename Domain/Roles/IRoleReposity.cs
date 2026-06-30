
using Domain.Abstractions;
using Domain.Users;

namespace Domain.Roles;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(RoleId id, CancellationToken cancellationToken = default);
    
    void Add(Role role);

    void  Update(Role role);

    void Remove(Role role);
    
    
    Task<List<Role>?> GetByIdsAsync(List<RoleId> ids, CancellationToken cancellationToken = default);
    
    Task<List<Role>?> Pagination(int take, int skip, string? search, CancellationToken cancellationToken = default);
    Task<List<Role>?> GetAll( CancellationToken cancellationToken = default);
    
    IQueryable<Role> GetEntitiesAsQueryable();
    
    Task<PagedList<Role>> GetAllPaged(PagedQuery<Role, RoleId> request,
        IQueryable<Role>? queryable = null);
}