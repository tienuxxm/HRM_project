using Domain.Abstractions;
using Domain.Roles;
using Domain.Users;

namespace Domain.UserToRoles;

public sealed class UserToRole : Entity<UserToRoleId>
{
    
   public UserId UserId { get; private set; }
   public User User { get; private set; }
   public RoleId RoleId { get; private set; }
   public Role Role { get; private set; }
   public DateTime CreatedDate { get; private set; }
    
    
    private UserToRole(
        UserToRoleId id,
        RoleId roleId, 
        UserId userId,
        DateTime createdDate
        )
    {
        Id = id;
        UserId = userId;
        RoleId = roleId;
        CreatedDate = createdDate;
    }

    private UserToRole()
    {
        
    }

    public static UserToRole Create( 
        RoleId roleId, 
        UserId userId,
        DateTime  createdDate
        )
    {
        return new UserToRole(UserToRoleId.New(),roleId, userId, createdDate); 
    }
    
    
    public void Update(
        RoleId roleId, 
        UserId userId
        )
    {
        UserId = userId;
        RoleId = roleId;
    }
    
}