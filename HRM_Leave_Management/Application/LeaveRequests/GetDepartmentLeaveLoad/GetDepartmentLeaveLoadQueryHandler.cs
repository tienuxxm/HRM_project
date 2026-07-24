using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Departments;
using Domain.LeaveRequests;
using Microsoft.EntityFrameworkCore;

namespace Application.LeaveRequests.GetDepartmentLeaveLoad;

internal sealed class GetDepartmentLeaveLoadQueryHandler
    : IQueryHandler<GetDepartmentLeaveLoadQuery, List<DepartmentLeaveLoadItem>>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILeaveRequestRepository _leaveRequestRepository;

    public GetDepartmentLeaveLoadQueryHandler(
        IDepartmentRepository departmentRepository,
        ILeaveRequestRepository leaveRequestRepository)
    {
        _departmentRepository = departmentRepository;
        _leaveRequestRepository = leaveRequestRepository;
    }

    public async Task<Result<List<DepartmentLeaveLoadItem>>> Handle(
        GetDepartmentLeaveLoadQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var firstDayOfMonth = new DateOnly(today.Year, today.Month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        var departments = await _departmentRepository.GetEntitiesAsQueryable()
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (!departments.Any())
        {
            return Result.Success(new List<DepartmentLeaveLoadItem>());
        }

        var activeLeaveRequests = await _leaveRequestRepository.GetEntitiesAsQueryable()
            .AsNoTracking()
            .Include(lr => lr.Employee)
            .Where(lr => lr.Employee.IsActive &&
                         (lr.Status == LeaveRequestStatus.Pending || lr.Status == LeaveRequestStatus.Approved) &&
                         lr.StartDate <= lastDayOfMonth && lr.EndDate >= firstDayOfMonth)
            .ToListAsync(cancellationToken);

        var departmentGroups = activeLeaveRequests
            .Where(lr => lr.Employee.DepartmentId != null)
            .GroupBy(lr => lr.Employee.DepartmentId!)
            .ToDictionary(g => g.Key, g => g.ToList());

        var maxLeaves = activeLeaveRequests.Count > 0 ? activeLeaveRequests.Count : 1;

        var items = new List<DepartmentLeaveLoadItem>();

        foreach (var dept in departments)
        {
            int leaveCount = departmentGroups.TryGetValue(dept.Id, out var deptLeaves) ? deptLeaves.Count : 0;
            decimal totalDays = departmentGroups.TryGetValue(dept.Id, out var deptLeaves2)
                ? deptLeaves2.Sum(l => l.Duration)
                : 0m;

            items.Add(new DepartmentLeaveLoadItem
            {
                DepartmentName = dept.Name,
                ActiveLeaveCount = leaveCount,
                TotalDays = totalDays,
                LoadPercentage = Math.Round((double)leaveCount / maxLeaves * 100, 1)
            });
        }

        var sortedItems = items
            .OrderByDescending(i => i.ActiveLeaveCount)
            .ThenBy(i => i.DepartmentName)
            .ToList();

        return Result.Success(sortedItems);
    }
}
