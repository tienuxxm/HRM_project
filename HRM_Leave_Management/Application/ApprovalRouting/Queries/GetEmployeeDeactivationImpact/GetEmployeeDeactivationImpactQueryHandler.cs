using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.ApprovalRouting;
using Domain.Employees;
using Domain.LeaveRequests;
using Domain.Positions;
using Microsoft.EntityFrameworkCore;

namespace Application.ApprovalRouting.Queries.GetEmployeeDeactivationImpact;

internal sealed class GetEmployeeDeactivationImpactQueryHandler
    : IQueryHandler<GetEmployeeDeactivationImpactQuery, EmployeeDeactivationImpactResponse>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveRequestApprovalAssignmentRepository _assignmentRepository;
    private readonly IApprovalRoutePolicyRepository _policyRepository;
    private readonly IPositionRepository _positionRepository;

    public GetEmployeeDeactivationImpactQueryHandler(
        IEmployeeRepository employeeRepository,
        ILeaveRequestApprovalAssignmentRepository assignmentRepository,
        IApprovalRoutePolicyRepository policyRepository,
        IPositionRepository positionRepository)
    {
        _employeeRepository = employeeRepository;
        _assignmentRepository = assignmentRepository;
        _policyRepository = policyRepository;
        _positionRepository = positionRepository;
    }

    public async Task<Result<EmployeeDeactivationImpactResponse>> Handle(
        GetEmployeeDeactivationImpactQuery request,
        CancellationToken cancellationToken)
    {
        var employeeId = new EmployeeId(request.EmployeeId);
        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
        if (employee == null)
        {
            return Result.Failure<EmployeeDeactivationImpactResponse>(EmployeeErrors.NotFound);
        }

        // 1. Query pending leave requests assigned to this approver (Historical Approved/Rejected/Canceled are strictly excluded)
        var pendingAssignments = await _assignmentRepository.GetPendingAssignmentsByApproverAsync(employeeId, cancellationToken);

        var affectedPendingRequests = pendingAssignments
            .Where(a => a.LeaveRequest != null && a.LeaveRequest.Status == LeaveRequestStatus.Pending)
            .Select(a => new AffectedPendingLeaveRequestDto(
                a.LeaveRequestId.Value,
                a.LeaveRequest?.Employee?.FullName ?? "Unknown",
                a.LeaveRequest?.LeaveType?.Name ?? "Leave",
                a.LeaveRequest!.StartDate,
                a.LeaveRequest.EndDate,
                a.LeaveRequest.Duration,
                a.LeaveRequest.Status.ToString()))
            .ToList();

        // 2. Query active Level Slots and Specific Rules with explicit EF Core Include loading
        var policies = await _policyRepository.GetEntitiesAsQueryable()
            .Include(p => p.Levels)
                .ThenInclude(l => l.Assignments)
            .Include(p => p.Rules)
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);

        var affectedLevelSlots = new List<AffectedLevelSlotDto>();
        var matchingRules = new List<(string PolicyName, ApprovalRouteRule Rule)>();

        foreach (var policy in policies)
        {
            foreach (var level in policy.Levels)
            {
                foreach (var assignment in level.Assignments.Where(a => a.AssignedEmployeeId == employeeId && a.IsActive))
                {
                    affectedLevelSlots.Add(new AffectedLevelSlotDto(
                        assignment.Id.Value,
                        level.Id.Value,
                        level.LevelName,
                        policy.Name,
                        assignment.EffectiveFrom,
                        assignment.EffectiveTo));
                }
            }

            foreach (var rule in policy.Rules.Where(r => r.SpecificApproverEmployeeId == employeeId && r.IsActive))
            {
                matchingRules.Add((policy.Name, rule));
            }
        }

        // 3. Resolve position names for matching rules (Item 2: No "All Positions" default)
        var positionIds = matchingRules
            .Select(r => r.Rule.RequesterPositionId)
            .Distinct()
            .ToList();

        var positionDict = await _positionRepository.GetEntitiesAsQueryable()
            .Where(p => positionIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

        var affectedSpecificRules = matchingRules
            .Select(mr => new AffectedSpecificRuleDto(
                mr.Rule.Id.Value,
                mr.PolicyName,
                positionDict.TryGetValue(mr.Rule.RequesterPositionId, out var posName) ? posName : mr.Rule.RequesterPositionId.Value.ToString()))
            .ToList();

        var response = new EmployeeDeactivationImpactResponse(
            employee.Id.Value,
            employee.FullName,
            affectedPendingRequests.Count,
            affectedPendingRequests,
            affectedLevelSlots.Count,
            affectedLevelSlots,
            affectedSpecificRules.Count,
            affectedSpecificRules);

        return Result.Success(response);
    }
}
