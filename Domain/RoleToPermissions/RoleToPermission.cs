using Domain.Abstractions;
using Domain.Permissions;
using Domain.Roles;

namespace Domain.RoleToPermissions;

public sealed class RoleToPermission : Entity<RoleToPermissionId>
{
    
   public PermissionId PermissionId { get; private set; }
   public Permission Permission { get; private set; }
   public RoleId RoleId { get; private set; }
   public Role Role { get; private set; }
   public DateTime CreatedDate { get; private set; }
    
    
    private RoleToPermission(
        RoleToPermissionId id,
        RoleId roleId, 
        PermissionId permissionId,
        DateTime createdDate
        )
    {
        Id = id;
        PermissionId = permissionId;
        RoleId = roleId;
        CreatedDate = createdDate;
    }

    private RoleToPermission()
    {
        
    }

    public static RoleToPermission Create( 
        RoleId roleId, 
        PermissionId permissionId,
        DateTime  createdDate
        )
    {
        return new RoleToPermission(RoleToPermissionId.New(),roleId, permissionId, createdDate); 
    }
    
    
    public void Update(
        RoleId roleId, 
        PermissionId permissionId
        )
    {
        PermissionId = permissionId;
        RoleId = roleId;
    }
    
}