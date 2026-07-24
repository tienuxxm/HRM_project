using Application.Abstractions.ApprovalRouting;
using Application.Abstractions.Authentication;
using Application.Abstractions.Role;
using Application.ApprovalRouting.Commands.ReassignPendingLeaveRequests;
using Domain.Abstractions;
using Domain.ApprovalRouting;
using Domain.Employees;
using Domain.LeaveRequests;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.ApprovalRouting.Services;

internal sealed class ApprovalReassignmentService : IApprovalReassignmentService
{
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveRequestApprovalAssignmentRepository _assignmentRepository;
    private readonly IApprovalRouteAuditLogRepository _auditLogRepository;
    private readonly IApprovalRouteResolverService _resolverService;
    private readonly IRoleService _roleService;

    public ApprovalReassignmentService(
        IUserContext userContext,
        IUserRepository userRepository,
        IEmployeeRepository employeeRepository,
        ILeaveRequestApprovalAssignmentRepository assignmentRepository,
        IApprovalRouteAuditLogRepository auditLogRepository,
        IApprovalRouteResolverService resolverService,
        IRoleService roleService)
    {
        _userContext = userContext;
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
        _assignmentRepository = assignmentRepository;
        _auditLogRepository = auditLogRepository;
        _resolverService = resolverService;
        _roleService = roleService;
    }

    public async Task<Result<ReassignPendingLeaveRequestsResponse>> ExecuteReassignmentAsync(
        ReassignPendingLeaveRequestsCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Strategy Validation
        bool isManual = request.NewApproverEmployeeId.HasValue && !request.AutoRerouteUsingResolver;
        bool isAuto = !request.NewApproverEmployeeId.HasValue && request.AutoRerouteUsingResolver;

        if (!isManual && !isAuto)
        {
            return Result.Failure<ReassignPendingLeaveRequestsResponse>(
                new Error("ApprovalRouting.InvalidReassignStrategy",
                    "Invalid reassign strategy: Must specify either a manual new approver OR set AutoRerouteUsingResolver to true, but not both or neither."));
        }

        var targetEmployeeId = new EmployeeId(request.TargetEmployeeId);
        var targetEmployee = await _employeeRepository.GetByIdAsync(targetEmployeeId, cancellationToken);
        if (targetEmployee == null)
        {
            return Result.Failure<ReassignPendingLeaveRequestsResponse>(EmployeeErrors.NotFound);
        }

        if (isManual && request.NewApproverEmployeeId!.Value == request.TargetEmployeeId)
        {
            return Result.Failure<ReassignPendingLeaveRequestsResponse>(
                new Error("ApprovalRouting.InvalidApprover", "Target approver and new approver cannot be the same employee."));
        }

        // 2. Manual Approver Validation
        Employee? manualNewApprover = null;
        if (isManual)
        {
            var validateResult = await ValidateManualApproverAsync(request.NewApproverEmployeeId!.Value, cancellationToken);
            if (validateResult.IsFailure)
            {
                return Result.Failure<ReassignPendingLeaveRequestsResponse>(validateResult.Error);
            }
            manualNewApprover = validateResult.Value;
        }

        // Get admin user executing action
        var adminIdentityId = _userContext.IdentityId;
        var adminUser = await _userRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(u => u.IdentityId == new IdentityId(adminIdentityId), cancellationToken);
        Guid adminUserId = adminUser?.Id.Value ?? Guid.Empty;

        // 3. Fetch pending assignments
        var pendingAssignments = await _assignmentRepository.GetPendingAssignmentsByApproverAsync(targetEmployeeId, cancellationToken);

        // Filter strictly for Pending requests and optional TargetLevelAssignmentId scope
        var assignmentsToProcess = pendingAssignments
            .Where(a => a.LeaveRequest != null && a.LeaveRequest.Status == LeaveRequestStatus.Pending)
            .Where(a => !request.TargetLevelAssignmentId.HasValue
                        || (a.SnapshotLevelAssignmentId != null && a.SnapshotLevelAssignmentId.Value == request.TargetLevelAssignmentId.Value))
            .ToList();

        if (!assignmentsToProcess.Any())
        {
            return Result.Success(new ReassignPendingLeaveRequestsResponse(0, 0, 0));
        }

        int reassignedCount = 0;
        int needsAttentionCount = 0;

        foreach (var assignment in assignmentsToProcess)
        {
            var previousApproverId = assignment.AssignedApproverEmployeeId;
            var leaveRequest = assignment.LeaveRequest!;

            if (isManual && manualNewApprover != null)
            {
                if (manualNewApprover.Id == leaveRequest.EmployeeId)
                {
                    assignment.MarkNeedsAttention(ApprovalAssignmentReason.OperatorManualReassigned);

                    var auditLog = ApprovalRouteAuditLog.LogAction(
                        leaveRequest.Id,
                        assignment.Id,
                        previousApproverId,
                        newApproverId: null,
                        ApprovalRouteAuditActionType.NeedsAttention,
                        oldStatus: ApprovalAssignmentStatus.Assigned.ToString(),
                        newStatus: ApprovalAssignmentStatus.NeedsAdminAttention.ToString(),
                        reasonCode: "ManualApproverIsRequesterSelfApprovalForbidden",
                        createdByUserId: adminUserId,
                        note: $"Manual reassignment failed: Specified approver {manualNewApprover.Id.Value} is the requester. Marked NeedsAdminAttention.");

                    _assignmentRepository.Update(assignment);
                    _auditLogRepository.Add(auditLog);
                    needsAttentionCount++;
                }
                else
                {
                    assignment.Reassign(
                        manualNewApprover.Id,
                        ApprovalAssignmentReason.OperatorManualReassigned);

                    var auditLog = ApprovalRouteAuditLog.LogAction(
                        leaveRequest.Id,
                        assignment.Id,
                        previousApproverId,
                        manualNewApprover.Id,
                        ApprovalRouteAuditActionType.Reassigned,
                        oldStatus: ApprovalAssignmentStatus.Assigned.ToString(),
                        newStatus: ApprovalAssignmentStatus.Assigned.ToString(),
                        reasonCode: ApprovalAssignmentReason.OperatorManualReassigned.ToString(),
                        createdByUserId: adminUserId,
                        note: $"Operator manual reassignment: {request.Reason}");

                    _assignmentRepository.Update(assignment);
                    _auditLogRepository.Add(auditLog);
                    reassignedCount++;
                }
            }
            else if (isAuto)
            {
                var requester = await _employeeRepository.GetByIdAsync(leaveRequest.EmployeeId, cancellationToken);
                if (requester == null)
                {
                    assignment.MarkNeedsAttention(ApprovalAssignmentReason.OperatorManualReassigned);

                    var nullRequesterAuditLog = ApprovalRouteAuditLog.LogAction(
                        leaveRequest.Id,
                        assignment.Id,
                        previousApproverId,
                        newApproverId: null,
                        ApprovalRouteAuditActionType.NeedsAttention,
                        oldStatus: ApprovalAssignmentStatus.Assigned.ToString(),
                        newStatus: ApprovalAssignmentStatus.NeedsAdminAttention.ToString(),
                        reasonCode: "RequesterNotFoundDuringAutoReroute",
                        createdByUserId: adminUserId,
                        note: "Requester employee record was not found during auto-reroute.");

                    _assignmentRepository.Update(assignment);
                    _auditLogRepository.Add(nullRequesterAuditLog);
                    needsAttentionCount++;
                    continue;
                }

                ApprovalRouteLevelAssignmentId? excludedAssignmentId = request.TargetLevelAssignmentId.HasValue
                    ? new ApprovalRouteLevelAssignmentId(request.TargetLevelAssignmentId.Value)
                    : null;

                var resolutionResult = await _resolverService.ResolveApproverAsync(
                    requester,
                    cancellationToken,
                    excludedLevelAssignmentId: excludedAssignmentId);

                if (resolutionResult.IsSuccess && resolutionResult.AssignedApprover != null)
                {
                    var newApprover = resolutionResult.AssignedApprover;
                    var reason = resolutionResult.CandidateId == null
                        ? ApprovalAssignmentReason.SpecificEmployeeOverride
                        : (resolutionResult.PriorityOrder == 1
                            ? ApprovalAssignmentReason.DirectLevelMatch
                            : ApprovalAssignmentReason.SuperiorLevelEscalated);

                    assignment.Reassign(
                        newApprover.Id,
                        reason,
                        resolutionResult.PolicyId,
                        resolutionResult.RuleId,
                        resolutionResult.CandidateId,
                        resolutionResult.LevelAssignmentId);

                    var auditLog = ApprovalRouteAuditLog.LogAction(
                        leaveRequest.Id,
                        assignment.Id,
                        previousApproverId,
                        newApprover.Id,
                        ApprovalRouteAuditActionType.Reassigned,
                        oldStatus: ApprovalAssignmentStatus.Assigned.ToString(),
                        newStatus: ApprovalAssignmentStatus.Assigned.ToString(),
                        reasonCode: reason.ToString(),
                        createdByUserId: adminUserId,
                        note: $"Auto-rerouted using resolver engine: {request.Reason}");

                    _assignmentRepository.Update(assignment);
                    _auditLogRepository.Add(auditLog);
                    reassignedCount++;
                }
                else
                {
                    assignment.MarkNeedsAttention(ApprovalAssignmentReason.OperatorManualReassigned);

                    var auditLog = ApprovalRouteAuditLog.LogAction(
                        leaveRequest.Id,
                        assignment.Id,
                        previousApproverId,
                        newApproverId: null,
                        ApprovalRouteAuditActionType.NeedsAttention,
                        oldStatus: ApprovalAssignmentStatus.Assigned.ToString(),
                        newStatus: ApprovalAssignmentStatus.NeedsAdminAttention.ToString(),
                        reasonCode: "ResolverFailedNoValidApprover",
                        createdByUserId: adminUserId,
                        note: $"Auto-reroute failed to find valid approver: {resolutionResult.ErrorMessage}");

                    _assignmentRepository.Update(assignment);
                    _auditLogRepository.Add(auditLog);
                    needsAttentionCount++;
                }
            }
        }

        // NOTE: SaveChangesAsync is NOT called here!
        // Calling Handler is the SOLE COMMIT OWNER for UnitOfWork!

        return Result.Success(new ReassignPendingLeaveRequestsResponse(
            assignmentsToProcess.Count,
            reassignedCount,
            needsAttentionCount));
    }

    private async Task<Result<Employee>> ValidateManualApproverAsync(
        Guid newApproverId,
        CancellationToken cancellationToken)
    {
        var employeeId = new EmployeeId(newApproverId);
        var newApprover = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
        if (newApprover == null || !newApprover.IsActive)
        {
            return Result.Failure<Employee>(
                new Error("ApprovalRouting.InvalidApprover", "The specified new approver employee is inactive or not found."));
        }

        if (newApprover.UserId == null)
        {
            return Result.Failure<Employee>(
                new Error("ApprovalRouting.InvalidApprover", "The specified new approver employee does not have a linked user account."));
        }

        var user = await _userRepository.GetByIdAsync(newApprover.UserId, cancellationToken);
        if (user == null || user.IsDeleted == true || user.IdentityId == null)
        {
            return Result.Failure<Employee>(
                new Error("ApprovalRouting.InvalidApprover", "The specified new approver's user account is inactive or disabled."));
        }

        var checkPerm = await _roleService.checkRoleExist(user.IdentityId.Value, "APPROVE_LEAVE_REQUEST", cancellationToken);
        if (checkPerm.IsFailure || !checkPerm.Value)
        {
            return Result.Failure<Employee>(
                new Error("ApprovalRouting.InvalidApprover", "The specified new approver does not possess the APPROVE_LEAVE_REQUEST permission."));
        }

        return Result.Success(newApprover);
    }
}
