using Application.Abstractions.Messaging;
using Application.Permissions.GetAll;
using Application.Roles.GetOne;
using Application.Users.GetOne;
using Domain.Abstractions;
using Domain.Roles;
using Microsoft.EntityFrameworkCore;

namespace Application.Roles.GetAllPaged;

public class GetAllRolePagedCommandHandler : ICommandHandler<GetAllRolePagedCommand, GetAllRolePagedResponse>
{
    private readonly IRoleRepository _roleRepository;

    public GetAllRolePagedCommandHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result<GetAllRolePagedResponse>> Handle(GetAllRolePagedCommand request,
        CancellationToken cancellationToken)
    {
        var roles = await _roleRepository.GetAllPaged(request,
            _roleRepository.GetEntitiesAsQueryable()
                .Include(x => x.Permissions)!
                .ThenInclude(x => x.Permission)
                .Include(x => x.Permissions)
                .ThenInclude(x => x.Permission)
            );
        var rolesDto = roles.Data.Select(role =>
        {
            var roleResponse = new RoleResponse()
            {
                Id = role.Id.Value,
                DisplayName = role.ResourceName.Value,
                ResourceName = role.ResourceName.Value,
                CreatedDate = role.CreatedDate,
                Users = new List<UserResponse>(),
                Permissions = new List<PermissionResponse>(),
            };
            
            if (role.Users != null)
            {
                var users = role.Users.Select(utr => new UserResponse
                {
                    Id = utr.User.Id.Value,
                    Fullname = utr.User.Name.Value,
                    Email = utr.User.Email?.Value,
                    PhoneNumber = utr.User.PhoneNumber?.Value,
                    Username = utr.User.Username.Value,
                    CreatedAt = utr.User.CreatedAt,
                }).ToList();
                roleResponse.Users.AddRange(users);
            }

            if (role.Permissions != null)
            {
                var permissions = role.Permissions.Select(rtp => new PermissionResponse()
                {
                    Id = rtp.Permission.Id.Value,
                    ResourceName = rtp.Permission.ResourceName.Value,
                    DisplayName = rtp.Permission.DisplayName.Value,
                });
                roleResponse.Permissions.AddRange(permissions);
            }
            return roleResponse;
        }).ToList();
        return Result.Success(
            new GetAllRolePagedResponse(rolesDto, roles.TotalCount, roles.CurrentPage, roles.PageSize));
    }
}