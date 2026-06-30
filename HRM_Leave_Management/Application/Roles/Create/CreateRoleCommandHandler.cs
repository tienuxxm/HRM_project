using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Permissions;
using Domain.Roles;
using Domain.RoleToPermissions;
using Domain.Users;
using Domain.UserToRoles;
using DisplayName = Domain.Roles.DisplayName;
using ResourceName = Domain.Roles.ResourceName;

namespace Application.Roles.Create;

internal class CreateRoleCommandHandler : ICommandHandler<CreateRoleCommand, Guid>
{
    private readonly IUserToRoleRepository _userToRoleRepository;
    private readonly IRoleToPermissionRepository _roleToPermissionRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;
    
    public CreateRoleCommandHandler(
        IRoleRepository roleRepository,
        IUserToRoleRepository userToRoleRepository,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork,
        IRoleToPermissionRepository roleToPermissionRepository)
    {
        _roleRepository = roleRepository;
        _userToRoleRepository = userToRoleRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
        _roleToPermissionRepository = roleToPermissionRepository;
    }

    public async Task<Result<Guid>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = Role.Create(
            new DisplayName(request.DisplayName),
            new ResourceName(request.ResourceName),
            _dateTimeProvider.UtcNow
            );
        if ( request.UserIds is not null && request.UserIds.Count > 0 )
        {
            var lstUserToRole = request.UserIds
                .Select(id => UserToRole.Create(role.Id, new UserId(id), _dateTimeProvider.UtcNow))
                .ToList();
            
            _userToRoleRepository.AddRange(lstUserToRole);
        }

        if (request.PermissionIds != null && request.PermissionIds.Count > 0)
        {
            var lstRoleToPermission = request.PermissionIds.Select(id =>
                RoleToPermission.Create(role.Id, new PermissionId(id), _dateTimeProvider.UtcNow)).ToList();
            
            _roleToPermissionRepository.AddRange(lstRoleToPermission);
        }
        _roleRepository.Add(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return role.Id.Value;
    }
}