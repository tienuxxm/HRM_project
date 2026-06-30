using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Images;
using Domain.Partners;
using Domain.Products;
using Domain.Roles;
using Domain.Users;
using Domain.UserToRoles;
using Domain.Vouchers;
namespace Application.Users.Update;

internal class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand,User>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserToRoleRepository  _userToRoleRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;
    private Partner? _partner;
    private Image? _image;
    public UpdateUserCommandHandler(
        IUserRepository userRepository,
        IUserToRoleRepository userToRoleRepository,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork
        )
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _userToRoleRepository = userToRoleRepository;
        _dateTimeProvider = dateTimeProvider;
    }
    public async Task<Result<User>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user is null)
        {
            return Result.Failure<User>(UserErrors.NotFound);
        }

        if (request.Email != null)
        {
          var finUniqUserByEmail = await _userRepository.FindUniqEmail(request.UserId, new Email(request.Email));
          if (finUniqUserByEmail != null )
          {
              return Result.Failure<User>(UserErrors.DuplicateEmail);
          }
        }
        
        user.Update(request.Name, request.Email, request.PhoneNumber);
        if (user.Roles != null)
        {
            _userToRoleRepository.RemoveRange(user.Roles);
        }
        if (request.RoleIds != null)
        {
            var userToRoles = request.RoleIds
                .Select(id => UserToRole.Create(new RoleId(id), user.Id, _dateTimeProvider.UtcNow))
                .ToList();
            user.UpdateRoles(userToRoles);
            _userToRoleRepository.AddRange(userToRoles);
        }
        
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user;
    }
}
