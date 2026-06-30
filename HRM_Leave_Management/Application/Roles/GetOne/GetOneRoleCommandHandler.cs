using Application.Abstractions.Messaging;
using Application.Permissions.GetAll;
using Application.Users.GetOne;
using Domain.Abstractions;
using Domain.Roles;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.Roles.GetOne;

internal sealed class GetOneRoleCommandHandler : ICommandHandler<GetOneRoleCommand,RoleResponse>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GetOneRoleCommandHandler(
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork; 
    }

    public async Task<Result<RoleResponse>> Handle(GetOneRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetEntitiesAsQueryable()
            .Include(r => r.Permissions)
            .ThenInclude(r => r.Permission)
            .FirstOrDefaultAsync(r => r.Id.Equals(request.RoleId));
        if (role is null)
        {
            return Result.Failure<RoleResponse>(UserErrors.NotFound);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        var roleResponse = new RoleResponse
        {
            Id = role.Id.Value,
            DisplayName = role.ResourceName.Value,
            ResourceName = role.ResourceName.Value,
            CreatedDate = role.CreatedDate,
            Users =  new List<UserResponse>(),
            Permissions = new List<PermissionResponse>()
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
    }
}
