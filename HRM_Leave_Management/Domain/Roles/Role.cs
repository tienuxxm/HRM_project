using Domain.Abstractions;
using Domain.RoleToPermissions;
using Domain.UserToRoles;

namespace Domain.Roles;

public sealed class Role : Entity<RoleId>
{
    
    public ResourceName ResourceName { get; private set; }
    
    public DisplayName DisplayName { get; private set; }
    public DateTime CreatedDate { get; private set; }
    
    public List<UserToRole>? Users { get; private set; }
    public List<RoleToPermission>? Permissions { get; private set; }
    
    
    private Role(
        RoleId id, 
        DisplayName displayName,
        ResourceName resourceName,
        DateTime createdDate
        )
    {
        Id = id;
        DisplayName = displayName;
        ResourceName = resourceName;
        CreatedDate = createdDate;
    }

    private Role()
    {
        
    }

    public static Role Create( 
        DisplayName displayName,
        ResourceName resourceName,
        DateTime createdDate
        )
    {
        return new Role(RoleId.New(),displayName, resourceName, createdDate); 
    }
    
    
    public void Update(
        DisplayName? displayName,
        ResourceName? resourceName
    
        )
    {
        DisplayName = displayName ?? DisplayName;
        ResourceName = resourceName ?? ResourceName;
    }
    
}