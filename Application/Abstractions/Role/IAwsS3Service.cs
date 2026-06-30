using Domain.Abstractions;

namespace Application.Abstractions.Role;

public interface IRoleService
{
    Task<Result<Boolean>> checkRoleExist(string identityId, string permission,CancellationToken cancellationToken);
    
}