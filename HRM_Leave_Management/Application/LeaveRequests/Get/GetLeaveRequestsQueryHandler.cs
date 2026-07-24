using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Abstractions.Role;
using Domain.Abstractions;
using Domain.ApprovalRouting;
using Domain.Employees;
using Domain.LeaveRequests;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.LeaveRequests.Get;

/// <summary>
/// Get Leave Requests Query Handler (Phase 8 Deprecation & Dynamic Routing):
///   - Dynamic Approval Routing (LeaveRequestApprovalAssignment) is the SOLE SOURCE OF TRUTH for runtime approval scoping and CanApprove evaluation.
///   - Legacy LeaveApproverAssignment fallback is completely retired.
///   - Admin/HR role provides global request visibility for administration/reassignment, but does not calculate CanApprove unless assigned.
/// </summary>
internal sealed class GetLeaveRequestsQueryHandler : IQueryHandler<GetLeaveRequestsQuery, PagedList<LeaveRequestResponse>>
{
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveRequestApprovalAssignmentRepository _approvalAssignmentRepository;
    private readonly IUserContext _userContext;
    private readonly IRoleService _roleService;

    public GetLeaveRequestsQueryHandler(
        ILeaveRequestRepository leaveRequestRepository,
        IUserRepository userRepository,
        IEmployeeRepository employeeRepository,
        ILeaveRequestApprovalAssignmentRepository approvalAssignmentRepository,
        IUserContext userContext,
        IRoleService roleService)
    {
        _leaveRequestRepository = leaveRequestRepository;
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
        _approvalAssignmentRepository = approvalAssignmentRepository;
        _userContext = userContext;
        _roleService = roleService;
    }

    public async Task<Result<PagedList<LeaveRequestResponse>>> Handle(GetLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        string identityId = _userContext.IdentityId;

        // 1. Check APPROVE_LEAVE_REQUEST permission
        var hasApprovePermissionResult = await _roleService.checkRoleExist(identityId, "APPROVE_LEAVE_REQUEST", cancellationToken);
        bool hasApprovePermission = hasApprovePermissionResult.Value;

        // 2. Check VIEW_LEAVE_REQUEST permission (Employee self-view)
        var hasViewPermissionResult = await _roleService.checkRoleExist(identityId, "VIEW_LEAVE_REQUEST", cancellationToken);
        bool hasViewPermission = hasViewPermissionResult.Value;

        // 3. Check UPDATE_LEAVE_APPROVER_ASSIGNMENT permission (Admin/HR global oversight capability)
        var isAdminOrHRResult = await _roleService.checkRoleExist(identityId, "UPDATE_LEAVE_APPROVER_ASSIGNMENT", cancellationToken);
        bool isAdminOrHR = isAdminOrHRResult.Value;

        if (!hasApprovePermission && !hasViewPermission && !isAdminOrHR)
        {
            return Result.Success(new PagedList<LeaveRequestResponse>(new List<LeaveRequestResponse>(), 0, 1, request.PageSize));
        }

        var query = _leaveRequestRepository.GetEntitiesAsQueryable()
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .AsQueryable();

        // 4. Data access scoping
        if (!isAdminOrHR)
        {
            var user = await _userRepository.GetEntitiesAsQueryable()
                .FirstOrDefaultAsync(u => u.IdentityId == new IdentityId(identityId), cancellationToken);

            if (user == null)
            {
                return Result.Success(new PagedList<LeaveRequestResponse>(new List<LeaveRequestResponse>(), 0, 1, request.PageSize));
            }

            var employee = await _employeeRepository.GetEntitiesAsQueryable()
                .FirstOrDefaultAsync(e => e.UserId == user.Id && e.IsActive, cancellationToken);

            if (employee == null)
            {
                return Result.Success(new PagedList<LeaveRequestResponse>(new List<LeaveRequestResponse>(), 0, 1, request.PageSize));
            }

            if (hasViewPermission && !hasApprovePermission)
            {
                // Regular Employee: own requests only
                query = query.Where(lr => lr.EmployeeId == employee.Id);
            }
            else if (!hasViewPermission && hasApprovePermission)
            {
                // Approver only: requests where current employee is the assigned approver in LeaveRequestApprovalAssignment (excludes own)
                query = query.Where(lr =>
                    lr.EmployeeId != employee.Id &&
                    _approvalAssignmentRepository.GetEntitiesAsQueryable().Any(a =>
                        a.LeaveRequestId == lr.Id &&
                        a.AssignedApproverEmployeeId == employee.Id &&
                        a.AssignmentStatus == ApprovalAssignmentStatus.Assigned
                    )
                );
            }
            else if (hasViewPermission && hasApprovePermission)
            {
                // Employee + Approver: own requests OR requests assigned in LeaveRequestApprovalAssignment
                query = query.Where(lr =>
                    lr.EmployeeId == employee.Id ||
                    (lr.EmployeeId != employee.Id &&
                     _approvalAssignmentRepository.GetEntitiesAsQueryable().Any(a =>
                         a.LeaveRequestId == lr.Id &&
                         a.AssignedApproverEmployeeId == employee.Id &&
                         a.AssignmentStatus == ApprovalAssignmentStatus.Assigned
                     ))
                );
            }
        }
        else
        {
            // Admin/HR filter by EmployeeId if requested
            if (request.EmployeeId.HasValue)
            {
                var filterEmployeeId = new EmployeeId(request.EmployeeId.Value);
                query = query.Where(lr => lr.EmployeeId == filterEmployeeId);
            }
        }

        // Apply general filters
        if (request.LeaveTypeId.HasValue)
        {
            var filterLeaveTypeId = new Domain.LeaveTypes.LeaveTypeId(request.LeaveTypeId.Value);
            query = query.Where(lr => lr.LeaveTypeId == filterLeaveTypeId);
        }

        if (request.Status.HasValue)
        {
            var filterStatus = (LeaveRequestStatus)request.Status.Value;
            query = query.Where(lr => lr.Status == filterStatus);
        }

        // Pagination
        int page = request.Page > 0 ? request.Page : 1;
        int pageSize = request.PageSize > 0 ? Math.Min(request.PageSize, 100) : 5;

        int totalCount = await query.CountAsync(cancellationToken);

        int totalPages = totalCount > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 1;
        if (page > totalPages)
        {
            page = totalPages;
        }

        var rawList = await query
            .OrderByDescending(lr => lr.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Fetch processedBy user names
        var processedByUserGuids = rawList
            .Where(lr => lr.ProcessedBy.HasValue)
            .Select(lr => lr.ProcessedBy.Value)
            .Distinct()
            .ToList();

        var userDict = new Dictionary<Guid, string>();
        if (processedByUserGuids.Any())
        {
            var processedUserIds = processedByUserGuids.Select(g => new UserId(g)).ToList();
            var processedUsers = await _userRepository.GetEntitiesAsQueryable()
                .Where(u => processedUserIds.Contains(u.Id))
                .ToListAsync(cancellationToken);

            userDict = processedUsers.ToDictionary(u => u.Id.Value, u => u.Name.Value);
        }

        // Fetch Dynamic Approval Routing Assignments for response items
        var rawLeaveRequestIds = rawList.Select(lr => lr.Id).ToList();
        var approvalAssignments = await _approvalAssignmentRepository.GetEntitiesAsQueryable()
            .Include(a => a.AssignedApprover)
            .Where(a => rawLeaveRequestIds.Contains(a.LeaveRequestId))
            .ToListAsync(cancellationToken);

        var approvalAssignmentDict = approvalAssignments.ToDictionary(a => a.LeaveRequestId.Value);

        // Resolve current employee identity for CanApprove calculation
        Employee? currentEmployee = null;
        var currentUser = await _userRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(u => u.IdentityId == new IdentityId(identityId), cancellationToken);
        if (currentUser != null)
        {
            currentEmployee = await _employeeRepository.GetEntitiesAsQueryable()
                .FirstOrDefaultAsync(e => e.UserId == currentUser.Id && e.IsActive, cancellationToken);
        }

        var response = rawList.Select(lr => {
            bool canApprove = false;
            approvalAssignmentDict.TryGetValue(lr.Id.Value, out var dynamicAssignment);

            // CanApprove is strictly calculated from dynamic LeaveRequestApprovalAssignment (NO legacy fallback)
            if (hasApprovePermission && currentEmployee != null && lr.Status == LeaveRequestStatus.Pending && lr.EmployeeId != currentEmployee.Id)
            {
                canApprove = dynamicAssignment != null &&
                             dynamicAssignment.AssignmentStatus == ApprovalAssignmentStatus.Assigned &&
                             dynamicAssignment.AssignedApproverEmployeeId == currentEmployee.Id;
            }

            return new LeaveRequestResponse
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
                ProcessedByName = lr.ProcessedBy.HasValue && userDict.TryGetValue(lr.ProcessedBy.Value, out var name) ? name : null,
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
        }).ToList();

        var pagedResult = new PagedList<LeaveRequestResponse>(response, totalCount, page, pageSize);
        return Result.Success(pagedResult);
    }
}
