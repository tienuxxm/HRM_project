using Application.Abstractions.Authentication;
using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Roles;
using Domain.Users;
using Domain.UserToRoles;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Create;

internal class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly IUserToRoleRepository _userToRoleRepository;

    public CreateUserCommandHandler(
        IUserToRoleRepository userToRoleRepository,
        IUserRepository userRepository,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork, IAuthenticationService authenticationService)
    {
        _userRepository = userRepository;
        _userToRoleRepository = userToRoleRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
        _authenticationService = authenticationService;
    }

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(request.Email))
        {
            var findDuplicateEmail = await _userRepository.GetEntitiesAsQueryable()
                .FirstOrDefaultAsync(
                    x => x.Email == new Email(request.Email) && !(x.IsDeleted.HasValue && x.IsDeleted.Value),
                    cancellationToken);
            if (findDuplicateEmail != null)
                return Result.Failure<Guid>(UserErrors.DuplicateEmail);

            var findDuplicateUsername = await _userRepository.GetEntitiesAsQueryable()
                .FirstOrDefaultAsync(
                    x => x.Username == new Username(request.Username) && !(x.IsDeleted.HasValue && x.IsDeleted.Value),
                    cancellationToken);
            if (findDuplicateUsername != null)
                return Result.Failure<Guid>(UserErrors.DuplicateUsername);
        }

        var user = User.Create(
            new Name(request.Name),
            request.Email != null
                ? new Email(request.Email.ToLower())
                : new Email(request.Username + DateTime.UtcNow.ToString("hh:mm:ss") + "@random.com"),
            request.PhoneNumber != null ? new PhoneNumber(request.PhoneNumber) : null,
            new Username(request.Username.ToLower()),
            null,
            _dateTimeProvider.UtcNow);

        var identityIdResult = await _authenticationService.RegisterAsync(
            user,
            request.Password,
            cancellationToken);
        if (identityIdResult.IsFailure)
            return Result.Failure<Guid>(identityIdResult.Error);
        user.SetIdentityId(identityIdResult.Value);

        _userRepository.Add(user);
        if (request.RoleIds != null)
        {
            var userToRoles = new List<UserToRole>();
            foreach (var roleId in request.RoleIds)
            {
                var userToRole = UserToRole.Create(new RoleId(roleId), user.Id, _dateTimeProvider.UtcNow);
                userToRoles.Add(userToRole);
            }

            _userToRoleRepository.AddRange(userToRoles);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return user.Id.Value;
    }
}