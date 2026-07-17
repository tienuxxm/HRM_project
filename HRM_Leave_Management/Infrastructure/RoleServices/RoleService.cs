using Application.Abstractions.Role;
using Domain.Abstractions;
using Domain.Employees;
using Domain.Permissions;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using DisplayName = Domain.Permissions.DisplayName;

namespace Infrastructure.RoleServices;

public class RoleService : IRoleService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public RoleService(IUserRepository userRepository, IEmployeeRepository employeeRepository)
    {
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<Result<Boolean>> checkRoleExist(string identityId, string permission,CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetEntitiesAsQueryable()
            .Include(x => x.Roles)
            .ThenInclude(x => x.Role)
            .ThenInclude(x => x.Permissions)
            .ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(x => 
                    x.IdentityId.Equals(new IdentityId(identityId))
                , cancellationToken);
        if (user is null || (user.IsDeleted.HasValue && user.IsDeleted.Value))
        {
            return false;
        }

        var employee = await _employeeRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(e => e.UserId == user.Id, cancellationToken);
        if (employee is not null && !employee.IsActive)
        {
            return false;
        }

        var permissions = user.Roles.ToList()
            .Select(item => item.Role.Permissions).ToList()
            .Select(item => item.ToList().Select(x => x.Permission).ToList()).ToList();
        var isHavePermission =  permissions.Exists(x => x.Exists(y => y.ResourceName.Equals(new ResourceName(permission)) ));
        if (!isHavePermission)
        {
            return false;
        }

        return true;
    }
   
}