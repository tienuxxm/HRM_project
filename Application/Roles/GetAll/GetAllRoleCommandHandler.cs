using Application.Abstractions.Messaging;
using Application.Roles.GetOne;
using Application.Users.GetOne;
using Domain.Abstractions;
using Domain.Roles;

namespace Application.Roles.GetAll;

internal sealed class GetAllRoleCommandHandler : ICommandHandler<GetAllRoleCommand, List<RoleResponse>?>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private List<RoleResponse>? _roleResponse;
    private List<Role>? _roles;

    public GetAllRoleCommandHandler(
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<RoleResponse>?>> Handle(GetAllRoleCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Take.HasValue && request.Skip.HasValue)
        {
            _roles = await _roleRepository.Pagination(request.Take.Value, request.Skip.Value, request.Search);
        }

        {
            _roles = await _roleRepository.GetAll();
        }
        if (_roles != null)
        {
            _roleResponse = _roles.Select(r =>
            {
                var role = new RoleResponse()
                {
                    Id = r.Id.Value,
                    DisplayName = r.DisplayName.Value,
                    ResourceName = r.ResourceName.Value,
                    CreatedDate = r.CreatedDate,
                    Users = new List<UserResponse>()
                };

                if (r.Users != null)
                {
                    var users = r.Users.Select(utr => new UserResponse
                    {
                        Id = utr.User.Id.Value,
                        Fullname = utr.User.Name.Value,
                        Email = utr.User.Email?.Value,
                        PhoneNumber = utr.User.PhoneNumber?.Value,
                        Username = utr.User.Username.Value,
                        CreatedAt = utr.User.CreatedAt,
                    }).ToList();
                    role.Users.AddRange(users);
                }

                return role;
            }).ToList();
        }


        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _roleResponse;
    }
}