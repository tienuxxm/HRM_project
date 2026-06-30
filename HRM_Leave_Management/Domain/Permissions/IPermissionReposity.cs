
namespace Domain.Permissions;

public interface IPermissionRepository
{
    Task<Permission?> GetByIdAsync(PermissionId id, CancellationToken cancellationToken = default);
    
    void Add(Permission role);

    void  Update(Permission role);

    void Remove(Permission role);
    
    
    Task<List<Permission>?> Pagination(int take, int skip, string? search, CancellationToken cancellationToken = default);
    Task<List<Permission>?> GetAll( CancellationToken cancellationToken = default);
    
    IQueryable<Permission> GetEntitiesAsQueryable();
}