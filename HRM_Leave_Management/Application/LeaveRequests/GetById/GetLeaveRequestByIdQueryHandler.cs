using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Abstractions.Role;
using Application.LeaveRequests.Get;
using Domain.Abstractions;
using Domain.Employees;
using Domain.LeaveRequests;
using Domain.LeaveApproverAssignments;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.LeaveRequests.GetById;

internal sealed class GetLeaveRequestByIdQueryHandler : IQueryHandler<GetLeaveRequestByIdQuery, LeaveRequestResponse>
{
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveApproverAssignmentRepository _approverAssignmentRepository;
    private readonly IRoleService _roleService;

    public GetLeaveRequestByIdQueryHandler(
        ILeaveRequestRepository leaveRequestRepository,
        IUserRepository userRepository,
        IUserContext userContext,
        IEmployeeRepository employeeRepository,
        ILeaveApproverAssignmentRepository approverAssignmentRepository,
        IRoleService roleService)
    {
        _leaveRequestRepository = leaveRequestRepository;
        _userRepository = userRepository;
        _userContext = userContext;
        _employeeRepository = employeeRepository;
        _approverAssignmentRepository = approverAssignmentRepository;
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

        // Calculate security access and CanApprove
        var identityId = _userContext.IdentityId;
        var currentUser = await _userRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(u => u.IdentityId == new IdentityId(identityId), cancellationToken);

        if (currentUser == null)
        {
            return Result.Failure<LeaveRequestResponse>(LeaveRequestErrors.EmployeeNotFound);
        }

        // 1. MVP Confirmed: UPDATE_LEAVE_APPROVER_ASSIGNMENT duoc dung lam quyen quan tri cau hinh va global leave request visibility
        var isAdminOrHRResult = await _roleService.checkRoleExist(identityId, "UPDATE_LEAVE_APPROVER_ASSIGNMENT", cancellationToken);
        bool isAdminOrHR = isAdminOrHRResult.Value;

        var currentEmployee = await _employeeRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(e => e.UserId == currentUser.Id && e.IsActive, cancellationToken);

        // Neu khong phai Admin/HR ma khong co Employee thi tra ve loi EmployeeNotFound
        if (currentEmployee == null && !isAdminOrHR)
        {
            return Result.Failure<LeaveRequestResponse>(LeaveRequestErrors.EmployeeNotFound);
        }

        // 2. Kiem tra xem nguoi dung hien tai co quyen APPROVE_LEAVE_REQUEST hay khong
        var hasApprovePermissionResult = await _roleService.checkRoleExist(identityId, "APPROVE_LEAVE_REQUEST", cancellationToken);
        bool hasApprovePermission = hasApprovePermissionResult.Value;

        bool isOwner = currentEmployee != null && lr.EmployeeId == currentEmployee.Id;
        bool isApprover = false;

        var requester = lr.Employee;
        // isApprover yeu cau:
        // a) Co Employee map va co quyen APPROVE_LEAVE_REQUEST
        // b) Co matching active assignment
        // c) Khong phai chu don (de tranh tu duyet don cua minh)
        if (currentEmployee != null && hasApprovePermission && requester != null && lr.EmployeeId != currentEmployee.Id)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var assignments = await _approverAssignmentRepository.GetEntitiesAsQueryable()
                .Where(a => a.ApproverEmployeeId == currentEmployee.Id && a.IsActive)
                .ToListAsync(cancellationToken);

            isApprover = assignments.Any(a =>
                (a.TargetDepartmentId == null || a.TargetDepartmentId == requester.DepartmentId) &&
                (a.TargetPositionId == null || a.TargetPositionId == requester.PositionId) &&
                (!a.EffectiveFrom.HasValue || a.EffectiveFrom.Value <= today) &&
                (!a.EffectiveTo.HasValue || a.EffectiveTo.Value >= today)
            );
        }

        // Quyen xem chi tiet (Detail security):
        // Cho phep xem neu la chu don (isOwner), la nguoi duyet hop le (isApprover), hoac la Admin/HR (isAdminOrHR)
        if (!isOwner && !isApprover && !isAdminOrHR)
        {
            return Result.Failure<LeaveRequestResponse>(LeaveRequestErrors.NoPermission);
        }

        // Quyen phe duyet thuc te (CanApprove):
        // Chi xuat hien nut duyet va cho phep duyet khi don dang o trang thai Pending va nguoi dung la nguoi duyet hop le (isApprover)
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
            CanApprove = canApprove
        };

        return Result.Success(response);
    }
}
