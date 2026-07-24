using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Abstractions.Role;
using Domain.Abstractions;
using Domain.ApprovalRouting;
using Domain.Employees;
using Domain.LeaveRequests;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.LeaveRequests.GetLeaveStatusDistribution;

/// <summary>
/// Dashboard W2 handler: Returns leave request status distribution counts.
/// Scope rules (Phase 8 Dynamic Routing & Deprecation):
///   - Admin/HR (UPDATE_LEAVE_APPROVER_ASSIGNMENT): all requests EXCEPT own
///   - Approver (APPROVE_LEAVE_REQUEST): requests where current employee is the assigned approver in LeaveRequestApprovalAssignment (excludes own)
///   - Employee (VIEW_LEAVE_REQUEST only): own requests only
/// Sole Source of Truth: LeaveRequestApprovalAssignment (Legacy LeaveApproverAssignment retired).
/// </summary>
internal sealed class GetLeaveStatusDistributionQueryHandler
    : IQueryHandler<GetLeaveStatusDistributionQuery, LeaveStatusDistributionResult>
{
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveRequestApprovalAssignmentRepository _approvalAssignmentRepository;
    private readonly IUserContext _userContext;
    private readonly IRoleService _roleService;

    public GetLeaveStatusDistributionQueryHandler(
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

    public async Task<Result<LeaveStatusDistributionResult>> Handle(
        GetLeaveStatusDistributionQuery request, CancellationToken cancellationToken)
    {
        string identityId = _userContext.IdentityId;

        var isAdminOrHR = (await _roleService.checkRoleExist(identityId, "UPDATE_LEAVE_APPROVER_ASSIGNMENT", cancellationToken)).Value;
        var hasApprovePermission = (await _roleService.checkRoleExist(identityId, "APPROVE_LEAVE_REQUEST", cancellationToken)).Value;
        var hasViewPermission = (await _roleService.checkRoleExist(identityId, "VIEW_LEAVE_REQUEST", cancellationToken)).Value;

        if (!isAdminOrHR && !hasApprovePermission && !hasViewPermission)
        {
            return Result.Success(new LeaveStatusDistributionResult());
        }

        var query = _leaveRequestRepository.GetEntitiesAsQueryable()
            .Include(lr => lr.Employee)
            .AsQueryable();

        // Resolve current employee for scope filtering
        var user = await _userRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(u => u.IdentityId == new IdentityId(identityId), cancellationToken);

        if (user == null)
        {
            return Result.Success(new LeaveStatusDistributionResult());
        }

        var employee = await _employeeRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(e => e.UserId == user.Id && e.IsActive, cancellationToken);

        if (employee == null)
        {
            return Result.Success(new LeaveStatusDistributionResult());
        }

        if (isAdminOrHR)
        {
            // Admin/HR sees all requests EXCEPT their own (business rule)
            query = query.Where(lr => lr.EmployeeId != employee.Id);
        }
        else if (hasApprovePermission)
        {
            // Approver: sees requests where they are assigned approver in LeaveRequestApprovalAssignment
            query = query.Where(lr =>
                lr.EmployeeId != employee.Id &&
                _approvalAssignmentRepository.GetEntitiesAsQueryable().Any(a =>
                    a.LeaveRequestId == lr.Id &&
                    a.AssignedApproverEmployeeId == employee.Id &&
                    a.AssignmentStatus == ApprovalAssignmentStatus.Assigned
                )
            );
        }
        else
        {
            // Employee: own requests only
            query = query.Where(lr => lr.EmployeeId == employee.Id);
        }

        // Aggregate counts by status
        var statusGroups = await query
            .GroupBy(lr => lr.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var result = new LeaveStatusDistributionResult
        {
            ApprovedCount = statusGroups.FirstOrDefault(g => g.Status == LeaveRequestStatus.Approved)?.Count ?? 0,
            PendingCount = statusGroups.FirstOrDefault(g => g.Status == LeaveRequestStatus.Pending)?.Count ?? 0,
            RejectedCount = statusGroups.FirstOrDefault(g => g.Status == LeaveRequestStatus.Rejected)?.Count ?? 0,
            CanceledCount = statusGroups.FirstOrDefault(g => g.Status == LeaveRequestStatus.Canceled)?.Count ?? 0,
        };

        return Result.Success(result);
    }
}
