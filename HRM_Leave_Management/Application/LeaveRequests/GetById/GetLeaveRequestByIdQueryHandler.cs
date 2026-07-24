using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Abstractions.Role;
using Application.LeaveRequests.Get;
using Domain.Abstractions;
using Domain.ApprovalRouting;
using Domain.Employees;
using Domain.LeaveRequests;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.LeaveRequests.GetById;

/// <summary>
/// Get Leave Request By Id Query Handler (Phase 8 Deprecation & Dynamic Routing):
///   - Dynamic Approval Routing (LeaveRequestApprovalAssignment) is the SOLE SOURCE OF TRUTH for isApprover and CanApprove.
///   - Legacy LeaveApproverAssignment fallback is completely retired.
///   - Admin/HR role provides global request detail visibility, but does not grant CanApprove/decision panel unless assigned.
/// </summary>
internal sealed class GetLeaveRequestByIdQueryHandler : IQueryHandler<GetLeaveRequestByIdQuery, LeaveRequestResponse>
{
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveRequestApprovalAssignmentRepository _approvalAssignmentRepository;
    private readonly IRoleService _roleService;

    public GetLeaveRequestByIdQueryHandler(
        ILeaveRequestRepository leaveRequestRepository,
        IUserRepository userRepository,
        IUserContext userContext,
        IEmployeeRepository employeeRepository,
        ILeaveRequestApprovalAssignmentRepository approvalAssignmentRepository,
        IRoleService roleService)
    {
        _leaveRequestRepository = leaveRequestRepository;
        _userRepository = userRepository;
        _userContext = userContext;
        _employeeRepository = employeeRepository;
        _approvalAssignmentRepository = approvalAssignmentRepository;
        _roleService = roleService;
    }

    public async Task<Result<LeaveRequestResponse>> Handle(GetLeaveRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var leaveRequestId = new LeaveRequestId(request.Id);
        var lr = await _leaveRequestRepository.GetEntitiesAsQueryable()
            .Include(x => x.Employee)
            .Include(x => x.LeaveType)
            .FirstOrDefaultAsync(x => x.Id == leaveRequestId, cancellationToken);

        if (lr == null)
        {
            return Result.Failure<LeaveRequestResponse>(LeaveRequestErrors.NotFound);
        }

        string? processedByName = null;
        if (lr.ProcessedBy.HasValue)
        {
            var user = await _userRepository.GetByIdAsync(new UserId(lr.ProcessedBy.Value), cancellationToken);
            processedByName = user?.Name?.Value;
        }

        // Load Approval Routing Assignment
        var dynamicAssignment = await _approvalAssignmentRepository.GetEntitiesAsQueryable()
            .Include(a => a.AssignedApprover)
            .FirstOrDefaultAsync(a => a.LeaveRequestId == leaveRequestId, cancellationToken);

        // Security access and CanApprove evaluation
        var identityId = _userContext.IdentityId;
        var currentUser = await _userRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(u => u.IdentityId == new IdentityId(identityId), cancellationToken);

        if (currentUser == null)
        {
            return Result.Failure<LeaveRequestResponse>(LeaveRequestErrors.EmployeeNotFound);
        }

        // Check Admin/HR global visibility capability
        var isAdminOrHRResult = await _roleService.checkRoleExist(identityId, "UPDATE_LEAVE_APPROVER_ASSIGNMENT", cancellationToken);
        bool isAdminOrHR = isAdminOrHRResult.Value;

        var currentEmployee = await _employeeRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(e => e.UserId == currentUser.Id && e.IsActive, cancellationToken);

        if (currentEmployee == null && !isAdminOrHR)
        {
            return Result.Failure<LeaveRequestResponse>(LeaveRequestErrors.EmployeeNotFound);
        }

        // Check APPROVE_LEAVE_REQUEST permission
        var hasApprovePermissionResult = await _roleService.checkRoleExist(identityId, "APPROVE_LEAVE_REQUEST", cancellationToken);
        bool hasApprovePermission = hasApprovePermissionResult.Value;

        bool isOwner = currentEmployee != null && lr.EmployeeId == currentEmployee.Id;
        bool isApprover = false;

        if (currentEmployee != null && hasApprovePermission && lr.EmployeeId != currentEmployee.Id)
        {
            // Strict Dynamic Approval Routing Assignment (NO legacy fallback)
            if (dynamicAssignment != null)
            {
                isApprover = dynamicAssignment.AssignmentStatus == ApprovalAssignmentStatus.Assigned &&
                             dynamicAssignment.AssignedApproverEmployeeId == currentEmployee.Id;
            }
        }

        // Detail security access: Owner, assigned approver, or Admin/HR oversight
        if (!isOwner && !isApprover && !isAdminOrHR)
        {
            return Result.Failure<LeaveRequestResponse>(LeaveRequestErrors.NoPermission);
        }

        // CanApprove: Decision panel is rendered ONLY when Pending AND current user is the strictly assigned approver
        bool canApprove = isApprover && lr.Status == LeaveRequestStatus.Pending;

        var response = new LeaveRequestResponse
        {
            Id = lr.Id.Value,
            EmployeeId = lr.EmployeeId.Value,
            EmployeeName = lr.Employee?.FullName ?? "Unknown",
            EmployeeCode = lr.Employee?.EmployeeCode ?? "Unknown",
            LeaveTypeId = lr.LeaveTypeId.Value,
            LeaveTypeName = lr.LeaveType?.Name ?? "Unknown",
            StartDate = lr.StartDate,
            EndDate = lr.EndDate,
            StartDayPart = lr.StartDayPart.ToString(),
            EndDayPart = lr.EndDayPart.ToString(),
            Duration = lr.Duration,
            Reason = lr.Reason,
            Status = lr.Status.ToString(),
            CreatedAt = lr.CreatedAt,
            ProcessedAt = lr.ProcessedAt,
            ProcessedBy = lr.ProcessedBy,
            ProcessedByName = processedByName,
            Comment = lr.Comment,
            CanApprove = canApprove,

            // Approval Routing Assignment Info
            AssignedApproverEmployeeId = dynamicAssignment?.AssignedApproverEmployeeId?.Value,
            AssignedApproverName = dynamicAssignment?.AssignedApprover?.FullName,
            AssignedApproverCode = dynamicAssignment?.AssignedApprover?.EmployeeCode,
            AssignmentStatus = dynamicAssignment?.AssignmentStatus.ToString(),
            AssignmentReason = dynamicAssignment?.AssignmentReason.ToString(),
            NeedsRoutingAttention = dynamicAssignment?.AssignmentStatus == ApprovalAssignmentStatus.NeedsAdminAttention,
            SnapshotPolicyId = dynamicAssignment?.SnapshotPolicyId?.Value,
            SnapshotRuleId = dynamicAssignment?.SnapshotRuleId?.Value
        };

        return Result.Success(response);
    }
}
