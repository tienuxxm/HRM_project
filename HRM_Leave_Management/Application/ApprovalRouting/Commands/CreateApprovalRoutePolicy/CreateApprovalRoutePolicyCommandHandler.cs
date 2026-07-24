using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Abstractions.Role;
using Domain.Abstractions;
using Domain.ApprovalRouting;
using Domain.Departments;
using Microsoft.EntityFrameworkCore;

namespace Application.ApprovalRouting.Commands.CreateApprovalRoutePolicy;

internal sealed class CreateApprovalRoutePolicyCommandHandler : ICommandHandler<CreateApprovalRoutePolicyCommand, Guid>
{
    private readonly IApprovalRoutePolicyRepository _policyRepository;
    private readonly IUserContext _userContext;
    private readonly IRoleService _roleService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateApprovalRoutePolicyCommandHandler(
        IApprovalRoutePolicyRepository policyRepository,
        IUserContext userContext,
        IRoleService roleService,
        IUnitOfWork unitOfWork)
    {
        _policyRepository = policyRepository;
        _userContext = userContext;
        _roleService = roleService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateApprovalRoutePolicyCommand request, CancellationToken cancellationToken)
    {
        var checkPerm = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_LEAVE_APPROVER_ASSIGNMENT", cancellationToken);
        if (!checkPerm.Value)
        {
            return Result.Failure<Guid>(ApprovalRouteErrors.NoPermission);
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure<Guid>(ApprovalRouteErrors.PolicyNameRequired);
        }

        DepartmentId? departmentId = request.DepartmentId.HasValue && request.DepartmentId.Value != Guid.Empty
            ? new DepartmentId(request.DepartmentId.Value)
            : null;

        // Check duplicate active department policy
        if (departmentId != null)
        {
            bool duplicateExists = await _policyRepository.GetEntitiesAsQueryable()
                .AnyAsync(p => p.IsActive && p.DepartmentId == departmentId, cancellationToken);

            if (duplicateExists)
            {
                return Result.Failure<Guid>(ApprovalRouteErrors.DuplicateActivePolicyForDepartment);
            }
        }
        else
        {
            // Company-level policy duplicate check
            bool duplicateCompanyExists = await _policyRepository.GetEntitiesAsQueryable()
                .AnyAsync(p => p.IsActive && p.DepartmentId == null, cancellationToken);

            if (duplicateCompanyExists)
            {
                return Result.Failure<Guid>(ApprovalRouteErrors.DuplicateActiveCompanyPolicy);
            }
        }

        var policy = ApprovalRoutePolicy.Create(departmentId, request.Name);
        _policyRepository.Add(policy);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(policy.Id.Value);
    }
}
