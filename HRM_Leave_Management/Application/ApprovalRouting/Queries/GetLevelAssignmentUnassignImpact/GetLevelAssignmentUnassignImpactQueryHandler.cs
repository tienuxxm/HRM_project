using Application.Abstractions.ApprovalRouting;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.ApprovalRouting;
using Domain.Employees;
using Domain.LeaveRequests;
using Microsoft.EntityFrameworkCore;

namespace Application.ApprovalRouting.Queries.GetLevelAssignmentUnassignImpact;

internal sealed class GetLevelAssignmentUnassignImpactQueryHandler
    : IQueryHandler<GetLevelAssignmentUnassignImpactQuery, LevelAssignmentUnassignImpactResponse>
{
    private readonly IApprovalRoutePolicyRepository _policyRepository;
    private readonly ILeaveRequestApprovalAssignmentRepository _leaveAssignmentRepository;
    private readonly IApprovalRouteResolverService _resolverService;
    private readonly IEmployeeRepository _employeeRepository;

    public GetLevelAssignmentUnassignImpactQueryHandler(
        IApprovalRoutePolicyRepository policyRepository,
        ILeaveRequestApprovalAssignmentRepository leaveAssignmentRepository,
        IApprovalRouteResolverService resolverService,
        IEmployeeRepository employeeRepository)
    {
        _policyRepository = policyRepository;
        _leaveAssignmentRepository = leaveAssignmentRepository;
        _resolverService = resolverService;
        _employeeRepository = employeeRepository;
    }

    public async Task<Result<LevelAssignmentUnassignImpactResponse>> Handle(
        GetLevelAssignmentUnassignImpactQuery request,
        CancellationToken cancellationToken)
    {
        var targetAssignmentId = new ApprovalRouteLevelAssignmentId(request.LevelAssignmentId);

        // Load policy rules, candidates, levels and assignments
        var policies = await _policyRepository.GetEntitiesAsQueryable()
            .Include(p => p.Rules)
                .ThenInclude(r => r.Candidates)
            .Include(p => p.Levels)
                .ThenInclude(l => l.Assignments)
            .AsNoTracking()
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);

        ApprovalRouteLevelAssignment? targetAssignment = null;
        ApprovalRouteLevel? targetLevel = null;
        ApprovalRoutePolicy? targetPolicy = null;

        foreach (var policy in policies)
        {
            foreach (var level in policy.Levels)
            {
                targetAssignment = level.Assignments.FirstOrDefault(a => a.Id == targetAssignmentId);
                if (targetAssignment != null)
                {
                    targetLevel = level;
                    targetPolicy = policy;
                    break;
                }
            }
            if (targetAssignment != null) break;
        }

        if (targetAssignment == null || targetLevel == null || targetPolicy == null)
        {
            return Result.Failure<LevelAssignmentUnassignImpactResponse>(
                new Error("ApprovalRouting.LevelAssignmentNotFound", "The specified level slot assignment was not found."));
        }

        var assignedEmployee = await _employeeRepository.GetByIdAsync(targetAssignment.AssignedEmployeeId, cancellationToken);
        string assignedEmployeeName = assignedEmployee?.FullName ?? "Assigned Employee";

        // Collect Candidate IDs matching targetLevel.Id under targetPolicy
        var targetCandidateIds = targetPolicy.Rules
            .SelectMany(r => r.Candidates)
            .Where(c => c.ApprovalRouteLevelId == targetLevel.Id)
            .Select(c => c.Id)
            .ToHashSet();

        // Query pending leave request assignments currently assigned to target employee
        var pendingAssignments = await _leaveAssignmentRepository.GetPendingAssignmentsByApproverAsync(targetAssignment.AssignedEmployeeId, cancellationToken);

        // Scope-strict filtering:
        // 1. SnapshotLevelAssignmentId == targetAssignment.Id
        // 2. OR (AssignedApproverEmployeeId == targetAssignment.AssignedEmployeeId AND SnapshotPolicyId == targetPolicy.Id AND Candidate matches targetLevel.Id)
        var slotPendingAssignments = pendingAssignments
            .Where(a => a.LeaveRequest != null &&
                        a.LeaveRequest.Status == LeaveRequestStatus.Pending &&
                        (a.SnapshotLevelAssignmentId == targetAssignment.Id ||
                         (a.SnapshotPolicyId == targetPolicy.Id &&
                          a.AssignedApproverEmployeeId == targetAssignment.AssignedEmployeeId &&
                          a.SnapshotCandidateId != null &&
                          targetCandidateIds.Contains(a.SnapshotCandidateId))))
            .ToList();

        var affectedDtos = new List<AffectedUnassignRequestDto>();
        int autoCount = 0;
        int needsAttentionCount = 0;

        foreach (var assign in slotPendingAssignments)
        {
            var lr = assign.LeaveRequest!;
            var requester = lr.Employee;

            bool isAuto = false;
            string proposedApprover = "None (Needs Admin)";
            string statusStr = "NEEDS_ADMIN_ATTENTION";

            if (requester != null)
            {
                var dryRunResult = await _resolverService.ResolveApproverAsync(
                    requester,
                    cancellationToken,
                    excludedLevelAssignmentId: targetAssignment.Id);

                if (dryRunResult.IsSuccess && dryRunResult.AssignedApprover != null)
                {
                    isAuto = true;
                    proposedApprover = dryRunResult.AssignedApprover.FullName ?? "Resolved Candidate";
                    statusStr = "AUTO RE-ROUTABLE";
                    autoCount++;
                }
                else
                {
                    needsAttentionCount++;
                }
            }
            else
            {
                needsAttentionCount++;
            }

            affectedDtos.Add(new AffectedUnassignRequestDto(
                lr.Id.Value,
                requester?.FullName ?? "Unknown Requester",
                lr.LeaveType?.Name ?? "Leave",
                lr.StartDate,
                lr.EndDate,
                lr.Duration,
                isAuto,
                proposedApprover,
                statusStr));
        }

        var response = new LevelAssignmentUnassignImpactResponse(
            targetAssignment.Id.Value,
            targetAssignment.AssignedEmployeeId.Value,
            targetLevel.LevelName,
            assignedEmployeeName,
            slotPendingAssignments.Count,
            autoCount,
            needsAttentionCount,
            affectedDtos);

        return Result.Success(response);
    }
}
