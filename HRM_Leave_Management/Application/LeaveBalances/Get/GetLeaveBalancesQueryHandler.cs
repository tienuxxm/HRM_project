using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Abstractions.Role;
using Domain.Abstractions;
using Domain.Employees;
using Domain.LeaveBalances;
using Domain.LeaveRequests;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.LeaveBalances.Get;

internal sealed class GetLeaveBalancesQueryHandler : IQueryHandler<GetLeaveBalancesQuery, PagedList<LeaveBalanceResponse>>
{
    private readonly ILeaveBalanceRepository _leaveBalanceRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IUserContext _userContext;
    private readonly IRoleService _roleService;

    public GetLeaveBalancesQueryHandler(
        ILeaveBalanceRepository leaveBalanceRepository,
        IUserRepository userRepository,
        IEmployeeRepository employeeRepository,
        ILeaveRequestRepository leaveRequestRepository,
        IUserContext userContext,
        IRoleService roleService)
    {
        _leaveBalanceRepository = leaveBalanceRepository;
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
        _leaveRequestRepository = leaveRequestRepository;
        _userContext = userContext;
        _roleService = roleService;
    }

    public async Task<Result<PagedList<LeaveBalanceResponse>>> Handle(GetLeaveBalancesQuery request, CancellationToken cancellationToken)
    {
        string identityId = _userContext.IdentityId;

        // 1. Kiểm tra xem user có quyền UPDATE_LEAVE_BALANCE (Admin/HR) hay không
        var hasUpdatePermissionResult = await _roleService.checkRoleExist(identityId, "UPDATE_LEAVE_BALANCE", cancellationToken);
        bool hasUpdatePermission = hasUpdatePermissionResult.Value;

        // 2. Kiểm tra xem user có quyền VIEW_LEAVE_BALANCE hay không
        var hasViewPermissionResult = await _roleService.checkRoleExist(identityId, "VIEW_LEAVE_BALANCE", cancellationToken);
        bool hasViewPermission = hasViewPermissionResult.Value;

        // Nếu không có bất kỳ quyền nào, trả về danh sách trống
        if (!hasUpdatePermission && !hasViewPermission)
        {
            return Result.Success(new PagedList<LeaveBalanceResponse>(new List<LeaveBalanceResponse>(), 0, 1, request.PageSize));
        }

        var query = _leaveBalanceRepository.GetEntitiesAsQueryable()
            .Include(lb => lb.Employee)
            .Include(lb => lb.LeaveType)
            .Where(lb => lb.IsActive);

        // 3. Nếu chỉ có quyền VIEW_LEAVE_BALANCE (nhân viên tự xem số dư của mình)
        if (!hasUpdatePermission && hasViewPermission)
        {
            // Thực hiện Flow mapping: IdentityId -> User.IdentityId -> User.Id -> Employee.UserId
            var user = await _userRepository.GetEntitiesAsQueryable()
                .FirstOrDefaultAsync(u => u.IdentityId == new IdentityId(identityId), cancellationToken);

            if (user == null)
            {
                return Result.Success(new PagedList<LeaveBalanceResponse>(new List<LeaveBalanceResponse>(), 0, 1, request.PageSize));
            }

            var employee = await _employeeRepository.GetEntitiesAsQueryable()
                .FirstOrDefaultAsync(e => e.UserId == user.Id && e.IsActive, cancellationToken);

            if (employee == null)
            {
                return Result.Success(new PagedList<LeaveBalanceResponse>(new List<LeaveBalanceResponse>(), 0, 1, request.PageSize));
            }

            // Ép buộc lọc theo EmployeeId của chính nhân viên này
            query = query.Where(lb => lb.EmployeeId == employee.Id);
        }
        else if (hasUpdatePermission)
        {
            // Nếu là Admin/HR (UPDATE_LEAVE_BALANCE), áp dụng filter EmployeeId nếu được truyền từ request
            if (request.EmployeeId.HasValue)
            {
                var filterEmployeeId = new EmployeeId(request.EmployeeId.Value);
                query = query.Where(lb => lb.EmployeeId == filterEmployeeId);
            }
        }

        // Áp dụng các filter chung khác
        if (request.LeaveTypeId.HasValue)
        {
            var filterLeaveTypeId = new Domain.LeaveTypes.LeaveTypeId(request.LeaveTypeId.Value);
            query = query.Where(lb => lb.LeaveTypeId == filterLeaveTypeId);
        }

        if (request.Year.HasValue)
        {
            query = query.Where(lb => lb.Year == request.Year.Value);
        }

        // Ordering
        query = query
            .OrderByDescending(lb => lb.Year)
            .ThenBy(lb => lb.Employee.FullName);

        // --- PAGINATION ---
        int page = request.Page > 0 ? request.Page : 1;
        int pageSize = request.PageSize > 0 ? Math.Min(request.PageSize, 100) : 5;

        int totalCount = await query.CountAsync(cancellationToken);

        // Nếu page vượt quá totalPages sau delete/filter, clamp về page cuối
        int totalPages = totalCount > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 1;
        if (page > totalPages)
        {
            page = totalPages;
        }

        var leaveBalances = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Tính PendingDays cho subset trong page
        var employeeIds = leaveBalances.Select(lb => lb.EmployeeId).Distinct().ToList();
        var leaveTypeIds = leaveBalances.Select(lb => lb.LeaveTypeId).Distinct().ToList();

        var pendingRequests = await _leaveRequestRepository.GetEntitiesAsQueryable()
            .Where(lr => lr.Status == LeaveRequestStatus.Pending &&
                         employeeIds.Contains(lr.EmployeeId) &&
                         leaveTypeIds.Contains(lr.LeaveTypeId))
            .ToListAsync(cancellationToken);

        var pendingDurationDict = pendingRequests
            .GroupBy(lr => new { lr.EmployeeId, lr.LeaveTypeId, Year = lr.StartDate.Year })
            .ToDictionary(
                g => g.Key,
                g => g.Sum(lr => lr.Duration)
            );

        var responseItems = leaveBalances.Select(lb =>
        {
            var key = new { lb.EmployeeId, lb.LeaveTypeId, Year = lb.Year };
            decimal pendingDays = pendingDurationDict.TryGetValue(key, out var duration) ? duration : 0m;

            return new LeaveBalanceResponse
            {
                Id = lb.Id.Value,
                EmployeeId = lb.EmployeeId.Value,
                EmployeeName = lb.Employee?.FullName ?? "Unknown",
                EmployeeCode = lb.Employee?.EmployeeCode ?? "Unknown",
                LeaveTypeId = lb.LeaveTypeId.Value,
                LeaveTypeName = lb.LeaveType?.Name ?? "Unknown",
                LeaveTypeCode = lb.LeaveType?.Code ?? "Unknown",
                Year = lb.Year,
                AllocatedDays = lb.AllocatedDays,
                UsedDays = lb.UsedDays,
                PendingDays = pendingDays
            };
        }).ToList();

        var pagedResult = new PagedList<LeaveBalanceResponse>(responseItems, totalCount, page, pageSize);
        return Result.Success(pagedResult);
    }
}
