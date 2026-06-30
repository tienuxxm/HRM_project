using Domain.Abstractions;
using Domain.RoleToPermissions;
using Domain.UserToRoles;

namespace Domain.Permissions;

public sealed class Permission : Entity<PermissionId>
{
    
    public ResourceName ResourceName { get; private set; }
    public Boolean IsDefault { get; private set; }
    public DisplayName DisplayName { get; private set; }
    public DateTime CreatedDate { get; private set; }
    
    public List<RoleToPermission>? Roles { get; private set; }
    
    
    private Permission(
        PermissionId id, 
        DisplayName displayName,
        ResourceName resourceName,
        Boolean isRead,
        Boolean isUpdate,
        Boolean isDefault,
        DateTime createdDate
        )
    {
        Id = id;
        DisplayName = displayName;
        ResourceName = resourceName;
        CreatedDate = createdDate;
        IsDefault = isDefault;
    }

    private Permission()
    {
        
    }

    public static Permission Create( 
        DisplayName displayName,
        ResourceName resourceName,
        Boolean isRead,
        Boolean isUpdate,
        Boolean isDefault,
        DateTime createdDate
        )
    {
        return new Permission(PermissionId.New(),displayName, resourceName, isRead,isUpdate, isDefault, createdDate); 
    }
    
    
    public void Update(
        DisplayName? displayName,
        ResourceName? resourceName,
        Boolean? isRead,
        Boolean? isUpdate
        )
    {
        DisplayName = displayName ?? DisplayName;
        ResourceName = resourceName ?? ResourceName;
    }
    
}