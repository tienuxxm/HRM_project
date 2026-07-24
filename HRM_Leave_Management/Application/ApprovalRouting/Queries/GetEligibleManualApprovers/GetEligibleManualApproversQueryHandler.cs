using Application.Abstractions.Messaging;
using Application.Abstractions.Role;
using Domain.Abstractions;
using Domain.ApprovalRouting;
using Domain.Employees;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.ApprovalRouting.Queries.GetEligibleManualApprovers;

internal sealed class GetEligibleManualApproversQueryHandler
    : IQueryHandler<GetEligibleManualApproversQuery, List<EligibleApproverDto>>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IApprovalRoutePolicyRepository _policyRepository;
    private readonly IRoleService _roleService;

    public GetEligibleManualApproversQueryHandler(
        IEmployeeRepository employeeRepository,
        IUserRepository userRepository,
        IApprovalRoutePolicyRepository policyRepository,
        IRoleService roleService)
    {
        _employeeRepository = employeeRepository;
        _userRepository = userRepository;
        _policyRepository = policyRepository;
        _roleService = roleService;
    }

    public async Task<Result<List<EligibleApproverDto>>> Handle(
        GetEligibleManualApproversQuery request,
        CancellationToken cancellationToken)
    {
        // Blocker 4: Require valid TargetLevelAssignmentId and policy context. Do NOT infer scope blindly.
        if (!request.TargetLevelAssignmentId.HasValue || request.TargetLevelAssignmentId.Value == Guid.Empty)
        {
            return Result.Failure<List<EligibleApproverDto>>(new Error(
                "ApprovalRouting.TargetAssignmentRequired",
                "TargetLevelAssignmentId is required for eligible manual approver resolution."));
        }

        var assignmentId = new ApprovalRouteLevelAssignmentId(request.TargetLevelAssignmentId.Value);
        var policy = await _policyRepository.GetEntitiesAsQueryable()
            .Include(p => p.Levels)
                .ThenInclude(l => l.Assignments)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Levels.Any(l => l.Assignments.Any(a => a.Id == assignmentId)), cancellationToken);

        if (policy == null)
        {
            return Result.Failure<List<EligibleApproverDto>>(new Error(
                "ApprovalRouting.PolicyContextNotFound",
                "Approval route level assignment or policy context was not found."));
        }

        var targetEmployeeId = new EmployeeId(request.TargetEmployeeId);

        var employeesQuery = _employeeRepository.GetEntitiesAsQueryable()
            .Include(e => e.Position)
            .Include(e => e.Department)
            .AsNoTracking()
            .Where(e => e.IsActive && e.Id != targetEmployeeId && e.UserId != null);

        // Blocker 4: Strict scope matching based on policy.DepartmentId config:
        // - If department policy (DepartmentId != null): filter active employees belonging strictly to the SAME department.
        // - If company-level policy (DepartmentId == null): filter active employees belonging strictly to company-level scope (DepartmentId == null).
        if (policy.DepartmentId != null)
        {
            employeesQuery = employeesQuery.Where(e => e.DepartmentId == policy.DepartmentId);
        }
        else
        {
            employeesQuery = employeesQuery.Where(e => e.DepartmentId == null);
        }

        var activeEmployees = await employeesQuery.ToListAsync(cancellationToken);
        var eligibleList = new List<EligibleApproverDto>();

        foreach (var emp in activeEmployees)
        {
            var user = await _userRepository.GetByIdAsync(emp.UserId!, cancellationToken);
            if (user == null || user.IsDeleted == true || user.IdentityId == null)
            {
                continue;
            }

            var permCheck = await _roleService.checkRoleExist(user.IdentityId.Value, "APPROVE_LEAVE_REQUEST", cancellationToken);
            if (permCheck.IsFailure || !permCheck.Value)
            {
                continue;
            }

            eligibleList.Add(new EligibleApproverDto(
                emp.Id.Value,
                emp.EmployeeCode,
                emp.FullName,
                emp.Position?.Name ?? "Staff",
                emp.Department?.Name ?? "Company-Level Scope"));
        }

        return Result.Success(eligibleList);
    }
}
