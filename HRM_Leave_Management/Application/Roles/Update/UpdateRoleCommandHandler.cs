using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Permissions;
using Domain.Roles;
using Domain.RoleToPermissions;
using Domain.Users;
using Domain.UserToRoles;
using Domain.Vouchers;
using DisplayName = Domain.Roles.DisplayName;
using ResourceName = Domain.Roles.ResourceName;

namespace Application.Roles.Update;

internal sealed class UpdateRoleCommandHandler : ICommandHandler<UpdateRoleCommand, Role>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUserToRoleRepository _userToRoleRepository;
    private readonly IRoleToPermissionRepository _roleToPermissionRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;
    public UpdateRoleCommandHandler(
        IRoleRepository roleRepository,
        IUserToRoleRepository userToRoleRepository,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork, IRoleToPermissionRepository roleToPermissionRepository)
    {
        _roleRepository = roleRepository;
        _userToRoleRepository = userToRoleRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
        _roleToPermissionRepository = roleToPermissionRepository;
    }
    public async Task<Result<Role>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(request.RoleId);
        if (role is null)
        {
            return Result.Failure<Role>(RoleErrors.NotFound);
        }

        var displayName = request.DisplayName != null ? new DisplayName(request.DisplayName) : null;
        var resourceName = request.ResourceName != null ? new ResourceName(request.ResourceName) : null;
        if (request.UserIds != null)
        {
            if (role.Users != null)
            {
                _userToRoleRepository.RemoveRange(role.Users);
            }
            var lstUserToRole = request.UserIds
                .Select(id => UserToRole.Create(role.Id, new UserId(id), _dateTimeProvider.UtcNow))
                .ToList();
            
            _userToRoleRepository.AddRange(lstUserToRole);
        }

        if (role.Permissions != null)
        {
            _roleToPermissionRepository.RemoveRange(role.Permissions);
        }
        if ( request.PermissionIds != null && request.PermissionIds.Count > 0)
        {
            var lstRoleToPermission = request.PermissionIds
                .Select(id => RoleToPermission.Create(role.Id, new PermissionId(id), _dateTimeProvider.UtcNow))
                .ToList();
            _roleToPermissionRepository.AddRange(lstRoleToPermission);
        }
        
      
        
        role.Update(displayName, resourceName);

        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return role;
    }
}
