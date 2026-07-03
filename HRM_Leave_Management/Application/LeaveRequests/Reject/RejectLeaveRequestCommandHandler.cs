using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Abstractions.Role;
using Application.Response;
using Domain.Abstractions;
using Domain.Employees;
using Domain.LeaveRequests;
using Domain.LeaveApproverAssignments;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.LeaveRequests.Reject;

internal sealed class RejectLeaveRequestCommandHandler : ICommandHandler<RejectLeaveRequestCommand, BooleanResponse>
{
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly ILeaveApproverAssignmentRepository _approverAssignmentRepository;
    private readonly IRoleService _roleService;
    private readonly IUnitOfWork _unitOfWork;

    public RejectLeaveRequestCommandHandler(
        IUserContext userContext,
        IUserRepository userRepository,
        IEmployeeRepository employeeRepository,
        ILeaveRequestRepository leaveRequestRepository,
        ILeaveApproverAssignmentRepository approverAssignmentRepository,
        IRoleService roleService,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
        _leaveRequestRepository = leaveRequestRepository;
        _approverAssignmentRepository = approverAssignmentRepository;
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

        // 2. Get leave request
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

        // 3. Get requester employee
        var requesterEmployee = await _employeeRepository.GetByIdAsync(leaveRequest.EmployeeId, cancellationToken);
        if (requesterEmployee == null)
        {
            return Result.Failure<BooleanResponse>(LeaveRequestErrors.EmployeeNotFound);
        }

        // 4. Check matching active approver assignment
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var assignments = await _approverAssignmentRepository.GetEntitiesAsQueryable()
            .Where(a => a.ApproverEmployeeId == approverEmployee.Id && a.IsActive)
            .ToListAsync(cancellationToken);

        var hasMatchingAssignment = assignments.Any(a =>
            (a.TargetDepartmentId == null || a.TargetDepartmentId == requesterEmployee.DepartmentId) &&
            (a.TargetPositionId == null || a.TargetPositionId == requesterEmployee.PositionId) &&
            (!a.EffectiveFrom.HasValue || a.EffectiveFrom.Value <= today) &&
            (!a.EffectiveTo.HasValue || a.EffectiveTo.Value >= today)
        );

        if (!hasMatchingAssignment)
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
