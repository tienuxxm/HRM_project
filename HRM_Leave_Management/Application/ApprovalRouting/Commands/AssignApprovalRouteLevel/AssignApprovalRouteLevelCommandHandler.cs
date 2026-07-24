using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Abstractions.Role;
using Domain.Abstractions;
using Domain.ApprovalRouting;
using Domain.Employees;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.ApprovalRouting.Commands.AssignApprovalRouteLevel;

internal sealed class AssignApprovalRouteLevelCommandHandler : ICommandHandler<AssignApprovalRouteLevelCommand, Guid>
{
    private readonly IApprovalRoutePolicyRepository _policyRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;
    private readonly IRoleService _roleService;
    private readonly IUnitOfWork _unitOfWork;

    public AssignApprovalRouteLevelCommandHandler(
        IApprovalRoutePolicyRepository policyRepository,
        IEmployeeRepository employeeRepository,
        IUserRepository userRepository,
        IUserContext userContext,
        IRoleService roleService,
        IUnitOfWork unitOfWork)
    {
        _policyRepository = policyRepository;
        _employeeRepository = employeeRepository;
        _userRepository = userRepository;
        _userContext = userContext;
        _roleService = roleService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(AssignApprovalRouteLevelCommand request, CancellationToken cancellationToken)
    {
        var checkPerm = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_LEAVE_APPROVER_ASSIGNMENT", cancellationToken);
        if (!checkPerm.Value)
        {
            return Result.Failure<Guid>(ApprovalRouteErrors.NoPermission);
        }

        var approverEmployeeId = new EmployeeId(request.ApproverEmployeeId);
        var approverEmployee = await _employeeRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(e => e.Id == approverEmployeeId && e.IsActive, cancellationToken);

        if (approverEmployee == null)
        {
            return Result.Failure<Guid>(ApprovalRouteErrors.ApproverEmployeeNotFound);
        }

        if (approverEmployee.UserId == null)
        {
            return Result.Failure<Guid>(ApprovalRouteErrors.ApproverUserNotLinked);
        }

        var linkedUser = await _userRepository.GetByIdAsync(approverEmployee.UserId, cancellationToken);
        if (linkedUser == null || linkedUser.IsDeleted == true)
        {
            return Result.Failure<Guid>(ApprovalRouteErrors.ApproverUserNotLinked);
        }

        var hasApprovePerm = await _roleService.checkRoleExist(linkedUser.IdentityId.Value, "APPROVE_LEAVE_REQUEST", cancellationToken);
        if (!hasApprovePerm.Value)
        {
            return Result.Failure<Guid>(ApprovalRouteErrors.ApproverNoApprovePermission);
        }

        // Load level slot and policy
        var levelId = new ApprovalRouteLevelId(request.LevelId);
        var policy = await _policyRepository.GetEntitiesAsQueryable()
            .Include(p => p.Levels)
                .ThenInclude(l => l.Assignments)
            .FirstOrDefaultAsync(p => p.Levels.Any(l => l.Id == levelId), cancellationToken);

        if (policy == null)
        {
            return Result.Failure<Guid>(ApprovalRouteErrors.LevelNotFound);
        }

        var targetLevel = policy.Levels.First(l => l.Id == levelId);

        // Validate department match for department-level policies
        if (policy.DepartmentId != null && approverEmployee.DepartmentId != policy.DepartmentId)
        {
            return Result.Failure<Guid>(ApprovalRouteErrors.ApproverDepartmentMismatch);
        }

        var currentUser = await _userRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(u => u.IdentityId == new IdentityId(_userContext.IdentityId), cancellationToken);

        Guid operatorUserId = currentUser?.Id.Value ?? Guid.Empty;
        var effectiveFrom = request.EffectiveFrom ?? DateOnly.FromDateTime(DateTime.Today);

        // Deactivate previous active slot assignments
        var activeAssignments = targetLevel.Assignments.Where(a => a.IsActive).ToList();
        foreach (var assignment in activeAssignments)
        {
            assignment.Deactivate(effectiveFrom, "Reassigned via policy configuration console");
        }

        var newAssignment = ApprovalRouteLevelAssignment.Create(
            targetLevel.Id,
            approverEmployee.Id,
            effectiveFrom,
            request.EffectiveTo,
            operatorUserId,
            request.Reason ?? "Initial level slot assignment via policy configuration console");

        targetLevel.AddAssignment(newAssignment);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(newAssignment.Id.Value);
    }
}
