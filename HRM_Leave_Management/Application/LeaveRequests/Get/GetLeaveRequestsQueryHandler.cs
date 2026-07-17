using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Abstractions.Role;
using Domain.Abstractions;
using Domain.Employees;
using Domain.LeaveRequests;
using Domain.LeaveApproverAssignments;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.LeaveRequests.Get;

internal sealed class GetLeaveRequestsQueryHandler : IQueryHandler<GetLeaveRequestsQuery, PagedList<LeaveRequestResponse>>
{
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveApproverAssignmentRepository _approverAssignmentRepository;
    private readonly IUserContext _userContext;
    private readonly IRoleService _roleService;

    public GetLeaveRequestsQueryHandler(
        ILeaveRequestRepository leaveRequestRepository,
        IUserRepository userRepository,
        IEmployeeRepository employeeRepository,
        ILeaveApproverAssignmentRepository approverAssignmentRepository,
        IUserContext userContext,
        IRoleService roleService)
    {
        _leaveRequestRepository = leaveRequestRepository;
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
        _approverAssignmentRepository = approverAssignmentRepository;
        _userContext = userContext;
        _roleService = roleService;
    }

    public async Task<Result<PagedList<LeaveRequestResponse>>> Handle(GetLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        string identityId = _userContext.IdentityId;

        // 1. Kiểm tra xem user có quyền APPROVE_LEAVE_REQUEST hay không
        var hasApprovePermissionResult = await _roleService.checkRoleExist(identityId, "APPROVE_LEAVE_REQUEST", cancellationToken);
        bool hasApprovePermission = hasApprovePermissionResult.Value;

        // 2. Kiểm tra xem user có quyền VIEW_LEAVE_REQUEST (Employee) hay không
        var hasViewPermissionResult = await _roleService.checkRoleExist(identityId, "VIEW_LEAVE_REQUEST", cancellationToken);
        bool hasViewPermission = hasViewPermissionResult.Value;

        // 3. UPDATE_LEAVE_APPROVER_ASSIGNMENT đang được dùng làm quyền quản trị cấu hình và global leave request visibility trong MVP Phase 3B.
        var isAdminOrHRResult = await _roleService.checkRoleExist(identityId, "UPDATE_LEAVE_APPROVER_ASSIGNMENT", cancellationToken);
        bool isAdminOrHR = isAdminOrHRResult.Value;

        // Nếu không có bất kỳ quyền nào (bao gồm quyền quản trị cấu hình / global visibility của Admin/HR), trả về danh sách rỗng
        if (!hasApprovePermission && !hasViewPermission && !isAdminOrHR)
        {
            return Result.Success(new PagedList<LeaveRequestResponse>(new List<LeaveRequestResponse>(), 0, 1, request.PageSize));
        }

        var query = _leaveRequestRepository.GetEntitiesAsQueryable()
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .AsQueryable();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // 4. Phân quyền truy cập dữ liệu
        if (!isAdminOrHR)
        {
            // Lấy employee hiện tại của user
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
                // Nhân viên thường chỉ thấy đơn của mình
                query = query.Where(lr => lr.EmployeeId == employee.Id);
            }
            else if (!hasViewPermission && hasApprovePermission)
            {
                // Chỉ có quyền duyệt (approver): thấy các đơn mình được phân công duyệt (không bao gồm đơn của bản thân vì không có quyền view)
                query = query.Where(lr =>
                    lr.EmployeeId != employee.Id &&
                    _approverAssignmentRepository.GetEntitiesAsQueryable().Any(a =>
                        a.ApproverEmployeeId == employee.Id &&
                        a.IsActive &&
                        (!a.EffectiveFrom.HasValue || a.EffectiveFrom.Value <= today) &&
                        (!a.EffectiveTo.HasValue || a.EffectiveTo.Value >= today) &&
                        (a.TargetDepartmentId == null || a.TargetDepartmentId == lr.Employee.DepartmentId) &&
                        (a.TargetPositionId == null || a.TargetPositionId == lr.Employee.PositionId)
                    )
                );
            }
            else if (hasViewPermission && hasApprovePermission)
            {
                // Vừa có quyền view vừa có quyền duyệt: thấy đơn của chính mình HOẶC các đơn được phân công duyệt
                query = query.Where(lr =>
                    lr.EmployeeId == employee.Id ||
                    (lr.EmployeeId != employee.Id &&
                     _approverAssignmentRepository.GetEntitiesAsQueryable().Any(a =>
                         a.ApproverEmployeeId == employee.Id &&
                         a.IsActive &&
                         (!a.EffectiveFrom.HasValue || a.EffectiveFrom.Value <= today) &&
                         (!a.EffectiveTo.HasValue || a.EffectiveTo.Value >= today) &&
                         (a.TargetDepartmentId == null || a.TargetDepartmentId == lr.Employee.DepartmentId) &&
                         (a.TargetPositionId == null || a.TargetPositionId == lr.Employee.PositionId)
                     ))
                );
            }
        }
        else
        {
            // Admin/HR có thể lọc theo EmployeeId nếu có filter
            if (request.EmployeeId.HasValue)
            {
                var filterEmployeeId = new EmployeeId(request.EmployeeId.Value);
                query = query.Where(lr => lr.EmployeeId == filterEmployeeId);
            }
        }

        // 4. Áp dụng các filter chung khác
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

        // 5. Áp dụng phân trang (Pagination)
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

        // 6. Lấy danh sách processedBy user IDs
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

        // 7. Lấy danh sách assignment của user hiện tại để tính CanApprove
        Employee? currentEmployee = null;
        List<LeaveApproverAssignment> currentEmployeeAssignments = new();
        var currentUser = await _userRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(u => u.IdentityId == new IdentityId(identityId), cancellationToken);
        if (currentUser != null)
        {
            currentEmployee = await _employeeRepository.GetEntitiesAsQueryable()
                .FirstOrDefaultAsync(e => e.UserId == currentUser.Id && e.IsActive, cancellationToken);
            if (currentEmployee != null)
            {
                var todayDate = DateOnly.FromDateTime(DateTime.UtcNow);
                currentEmployeeAssignments = await _approverAssignmentRepository.GetEntitiesAsQueryable()
                    .Where(a => a.ApproverEmployeeId == currentEmployee.Id && a.IsActive)
                    .ToListAsync(cancellationToken);
            }
        }

        var response = rawList.Select(lr => {
            bool canApprove = false;
            if (hasApprovePermission && currentEmployee != null && lr.Status == LeaveRequestStatus.Pending && lr.EmployeeId != currentEmployee.Id && lr.Employee != null)
            {
                canApprove = currentEmployeeAssignments.Any(a =>
                    (a.TargetDepartmentId == null || a.TargetDepartmentId == lr.Employee.DepartmentId) &&
                    (a.TargetPositionId == null || a.TargetPositionId == lr.Employee.PositionId) &&
                    (!a.EffectiveFrom.HasValue || a.EffectiveFrom.Value <= today) &&
                    (!a.EffectiveTo.HasValue || a.EffectiveTo.Value >= today)
                );
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
                CanApprove = canApprove
            };
        }).ToList();

        var pagedResult = new PagedList<LeaveRequestResponse>(response, totalCount, page, pageSize);
        return Result.Success(pagedResult);
    }
}
