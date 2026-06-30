using Application.Abstractions.Messaging;
using Application.Roles.GetOne;
using Domain.Abstractions;
using Domain.Users;

namespace Application.Users.GetOne;

internal sealed class GetOneUserQueryHandler : IQueryHandler<GetOneUserQuery, UserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GetOneUserQueryHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserResponse>> Handle(GetOneUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserResponse>(UserErrors.NotFound);
        }

        var userResponse = new UserResponse
        {
            Id = user.Id.Value,
            Fullname = user.Name.Value,
            Username = user.Username.Value,
            CreatedAt = user.CreatedAt,
            Email = user.Email?.Value,
            PhoneNumber = user.PhoneNumber?.Value,
            Roles = new List<RoleResponse>()
        };
        if (user.Roles != null)
        {
            var userToRoles = user.Roles.Select(utr => new RoleResponse
            {
                Id = utr.Role.Id.Value,
                DisplayName = utr.Role.DisplayName.Value,
                ResourceName = utr.Role.ResourceName.Value,
                CreatedDate = utr.Role.CreatedDate,
            }).ToList();

            userResponse.Roles.AddRange(userToRoles);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return userResponse;
    }
}