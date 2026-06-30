using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Permissions;

namespace Application.Permissions.GetAll;

internal sealed class GetAllPermissionCommandHandler : ICommandHandler<GetAllPermissionCommand, List<PermissionResponse>?>
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IUnitOfWork _unitOfWork;
    public GetAllPermissionCommandHandler( IPermissionRepository permissionRepository, IUnitOfWork unitOfWork)
    {
        _permissionRepository = permissionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<PermissionResponse>?>> Handle(GetAllPermissionCommand request,
        CancellationToken cancellationToken)
    {
        var permission = await _permissionRepository.GetAll();
        var permissionResponse = permission.Select(p => new PermissionResponse()
        {
            Id = p.Id.Value,
            ResourceName = p.ResourceName.Value,
            DisplayName = p.DisplayName.Value,
        }).ToList();
    
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return permissionResponse;
    }
}