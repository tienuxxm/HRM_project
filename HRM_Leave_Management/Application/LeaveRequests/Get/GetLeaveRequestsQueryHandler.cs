using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Abstractions.Role;
using Domain.Abstractions;
using Domain.Employees;
using Domain.LeaveRequests;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.LeaveRequests.Get;

internal sealed class GetLeaveRequestsQueryHandler : IQueryHandler<GetLeaveRequestsQuery, List<LeaveRequestResponse>>
{
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUserContext _userContext;
    private readonly IRoleService _roleService;

    public GetLeaveRequestsQueryHandler(
        ILeaveRequestRepository leaveRequestRepository,
        IUserRepository userRepository,
        IEmployeeRepository employeeRepository,
        IUserContext userContext,
        IRoleService roleService)
    {
        _leaveRequestRepository = leaveRequestRepository;
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
        _userContext = userContext;
        _roleService = roleService;
    }

    public async Task<Result<List<LeaveRequestResponse>>> Handle(GetLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        string identityId = _userContext.IdentityId;

        // 1. Kiểm tra xem user có quyền APPROVE_LEAVE_REQUEST (Admin/HR) hay không
        var hasApprovePermissionResult = await _roleService.checkRoleExist(identityId, "APPROVE_LEAVE_REQUEST", cancellationToken);
        bool hasApprovePermission = hasApprovePermissionResult.Value;

        // 2. Kiểm tra xem user có quyền VIEW_LEAVE_REQUEST (Employee) hay không
        var hasViewPermissionResult = await _roleService.checkRoleExist(identityId, "VIEW_LEAVE_REQUEST", cancellationToken);
        bool hasViewPermission = hasViewPermissionResult.Value;

        // Nếu không có bất kỳ quyền nào, trả về danh sách rỗng
        if (!hasApprovePermission && !hasViewPermission)
        {
            return Result.Success(new List<LeaveRequestResponse>());
        }

        var query = _leaveRequestRepository.GetEntitiesAsQueryable()
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .AsQueryable();

        // 3. Phân quyền truy cập dữ liệu
        if (!hasApprovePermission && hasViewPermission)
        {
            // Nhân viên bình thường chỉ xem được đơn của chính họ
            var user = await _userRepository.GetEntitiesAsQueryable()
                .FirstOrDefaultAsync(u => u.IdentityId == new IdentityId(identityId), cancellationToken);

            if (user == null)
            {
                return Result.Success(new List<LeaveRequestResponse>());
            }

            var employee = await _employeeRepository.GetEntitiesAsQueryable()
                .FirstOrDefaultAsync(e => e.UserId == user.Id && e.IsActive, cancellationToken);

            if (employee == null)
            {
                return Result.Success(new List<LeaveRequestResponse>());
            }

            query = query.Where(lr => lr.EmployeeId == employee.Id);
        }
        else if (hasApprovePermission)
        {
            // Admin/HR có thể lọc theo EmployeeId
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

        // 5. Query dữ liệu từ database trước
        var rawList = await query
            .OrderByDescending(lr => lr.CreatedAt)
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

        var response = rawList.Select(lr => new LeaveRequestResponse
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
            Comment = lr.Comment
        }).ToList();

        return Result.Success(response);
    }
}
