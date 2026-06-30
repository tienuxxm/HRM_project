
namespace Domain.UserToRoles;

public interface IUserToRoleRepository
{
    Task<UserToRole?> GetByIdAsync(UserToRoleId id, CancellationToken cancellationToken = default);
    
    Task<List<UserToRole>?> GetByIdsAsync(List<UserToRoleId> ids, CancellationToken cancellationToken = default);
    
    void Add(UserToRole userToRole); 
    void AddRange(List<UserToRole>  userToRoles);

    void Update(UserToRole userToRole);

    void Remove(UserToRole userToRole);
    void RemoveRange(List<UserToRole> userToRoles);
    
}