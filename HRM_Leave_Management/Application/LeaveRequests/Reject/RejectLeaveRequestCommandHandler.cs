using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Abstractions.Role;
using Application.Response;
using Domain.Abstractions;
using Domain.ApprovalRouting;
using Domain.Employees;
using Domain.LeaveRequests;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.LeaveRequests.Reject;

/// <summary>
/// Reject Leave Request Handler (Phase 8 Business Rule Correction & Dynamic Routing):
/// Business Rule:
///   - Admin / HR / Config Operators DO NOT have permission to approve/reject leave requests merely because they hold administrative configuration rights.
///   - Admin/HR role is strictly for routing administration, reassignment, and policy configuration.
///   - The ONLY person authorized to reject a pending leave request is the CURRENTLY ASSIGNED APPROVER in LeaveRequestApprovalAssignment.
/// Authorization Steps:
///   1. Check APPROVE_LEAVE_REQUEST permission for current user identity.
///   2. Resolve current user identity -> active Employee (approver identity).
///   3. Verify LeaveRequest is Pending and not self-rejection.
///   4. Verify LeaveRequestApprovalAssignment contains an active record where:
///      - LeaveRequestId == leaveRequest.Id
///      - AssignedApproverEmployeeId == approverEmployee.Id
///      - AssignmentStatus == ApprovalAssignmentStatus.Assigned
///   5. Execute rejection.
/// Sole Source of Truth: LeaveRequestApprovalAssignment (Phase 8 retired legacy LeaveApproverAssignment).
/// </summary>
internal sealed class RejectLeaveRequestCommandHandler : ICommandHandler<RejectLeaveRequestCommand, BooleanResponse>
{
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly ILeaveRequestApprovalAssignmentRepository _dynamicAssignmentRepository;
    private readonly IRoleService _roleService;
    private readonly IUnitOfWork _unitOfWork;

    public RejectLeaveRequestCommandHandler(
        IUserContext userContext,
        IUserRepository userRepository,
        IEmployeeRepository employeeRepository,
        ILeaveRequestRepository leaveRequestRepository,
        ILeaveRequestApprovalAssignmentRepository dynamicAssignmentRepository,
        IRoleService roleService,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
        _leaveRequestRepository = leaveRequestRepository;
        _dynamicAssignmentRepository = dynamicAssignmentRepository;
        _roleService = roleService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BooleanResponse>> Handle(RejectLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        // 1. Check APPROVE_LEAVE_REQUEST permission
        var identityId = _userContext.IdentityId;
        var hasPermissionResult = await _roleService.checkRoleExist(identityId, "APPROVE_LEAVE_REQUEST", cancellationToken);
        if (!hasPermissionResult.Value)
        {
            return Result.Failure<BooleanResponse>(LeaveRequestErrors.NoPermission);
        }

        // 2. Get current approver user and employee
        var user = await _userRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(u => u.IdentityId == new IdentityId(identityId), cancellationToken);
        if (user == null)
        {
            return Result.Failure<BooleanResponse>(LeaveRequestErrors.EmployeeNotFound);
        }

        var approverEmployee = await _employeeRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(e => e.UserId == user.Id && e.IsActive, cancellationToken);
        if (approverEmployee == null)
        {
            return Result.Failure<BooleanResponse>(LeaveRequestErrors.EmployeeNotFound);
        }

        // 3. Get leave request
        var leaveRequestId = new LeaveRequestId(request.LeaveRequestId);
        var leaveRequest = await _leaveRequestRepository.GetByIdAsync(leaveRequestId, cancellationToken);
        if (leaveRequest == null)
        {
            return Result.Failure<BooleanResponse>(LeaveRequestErrors.NotFound);
        }

        if (leaveRequest.Status != LeaveRequestStatus.Pending)
        {
            return Result.Failure<BooleanResponse>(new Error("LeaveRequest.NotPending", "Only pending leave requests can be rejected."));
        }

        // Cannot reject own leave request
        if (leaveRequest.EmployeeId == approverEmployee.Id)
        {
            return Result.Failure<BooleanResponse>(LeaveRequestErrors.CannotRejectSelf);
        }

        // 4. Strict Assigned Approver Verification: Must match current assigned approver in LeaveRequestApprovalAssignment
        var isAssignedApprover = await _dynamicAssignmentRepository.GetEntitiesAsQueryable().AnyAsync(a =>
            a.LeaveRequestId == leaveRequest.Id &&
            a.AssignedApproverEmployeeId == approverEmployee.Id &&
            a.AssignmentStatus == ApprovalAssignmentStatus.Assigned,
            cancellationToken);

        if (!isAssignedApprover)
        {
            return Result.Failure<BooleanResponse>(LeaveRequestErrors.NoApprovalAssignment);
        }

        // 5. Reject leave request
        leaveRequest.Reject(user.Id.Value, DateTime.UtcNow, request.Comment);

        _leaveRequestRepository.Update(leaveRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new BooleanResponse
        {
            Result = true,
            Message = "Leave request rejected successfully."
        });
    }
}
