using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Users;
using Domain.UserToRoles;

namespace Application.Users.Delete;

internal sealed class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly IUserToRoleRepository _userToRoleRepository;

    public DeleteUserCommandHandler(
        IUserRepository userRepository,
        IUserToRoleRepository userToRoleRepository,
        IUnitOfWork unitOfWork, IAuthenticationService authenticationService)
    {
        _userRepository = userRepository;
        _userToRoleRepository = userToRoleRepository;
        _unitOfWork = unitOfWork;
        _authenticationService = authenticationService;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null) return Result.Failure<Guid>(UserErrors.NotFound);
        user.Delete();
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (!string.IsNullOrEmpty(user.IdentityId.Value))
        {
            var deleteUser = await _authenticationService.DeleteUser(user.IdentityId.Value, cancellationToken);
            if (deleteUser.IsFailure)
                return Result.Failure(UserErrors.InvalidCredentials);
        }

        return Result.Success();
    }
}