namespace Domain.RoleToPermissions;

public interface IRoleToPermissionRepository
{
    Task<RoleToPermission?> GetByIdAsync(RoleToPermissionId id, CancellationToken cancellationToken = default);
    
    Task<List<RoleToPermission>?> GetByIdsAsync(List<RoleToPermissionId> ids, CancellationToken cancellationToken = default);
    
    void Add(RoleToPermission userToRole); 
    void AddRange(List<RoleToPermission>  userToRoles);

    void Update(RoleToPermission userToRole);

    void Remove(RoleToPermission userToRole);
    void RemoveRange(List<RoleToPermission> userToRoles);
    
}