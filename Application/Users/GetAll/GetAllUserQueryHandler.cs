using Application.Abstractions.Messaging;
using Application.Roles.GetOne;
using Application.Users.GetOne;
using Domain.Abstractions;
using Domain.Images;
using Domain.Users;
using Domain.Vouchers;

namespace Application.Users.GetAll;

internal sealed class GetAllUserQueryHandler : IQueryHandler<GetAllUserQuery, List<UserResponse>?>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private List<UserResponse>? _userResponses;

    public GetAllUserQueryHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<UserResponse>?>> Handle(GetAllUserQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.Pagination(request.Take, request.Skip);
        if (users != null)
        {
            _userResponses = users.Select(u =>
            {
                var userResponse = new UserResponse
                {
                    Id = u.Id.Value,
                    Fullname = u.Name.Value,
                    Username = u.Username.Value,
                    CreatedAt = u.CreatedAt,
                    Email = u.Email?.Value,
                    PhoneNumber = u.PhoneNumber?.Value,
                    Roles = new List<RoleResponse>()
                };
                if (u.Roles != null)
                {
                    var userToRoles = u.Roles.Select(utr => new RoleResponse
                    {
                        Id = utr.Role.Id.Value,
                        DisplayName = utr.Role.DisplayName.Value,
                        ResourceName = utr.Role.ResourceName.Value,
                        CreatedDate = utr.Role.CreatedDate,
                    }).ToList();

                    userResponse.Roles.AddRange(userToRoles);
                }

                return userResponse;
            }).ToList();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _userResponses;
    }
}