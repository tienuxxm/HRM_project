using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Partners;
using Domain.Roles;
using Domain.UserToRoles;

namespace Application.Roles.Delete;

internal sealed class DeleteRoleCommandHandler : ICommandHandler<DeleteRoleCommand>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUserToRoleRepository _userToRoleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRoleCommandHandler(
        IRoleRepository roleRepository,
        IUserToRoleRepository userToRoleRepository,
        IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _userToRoleRepository = userToRoleRepository;
        _unitOfWork = unitOfWork; 
    }

    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(request.RoleId);
        if (role is null)
        {
            return Result.Failure<Guid>(PartnerErrors.NotFound);
        }

        if (role.Users != null)
        {
            _userToRoleRepository.RemoveRange(role.Users);

        }
        _roleRepository.Remove(role);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
