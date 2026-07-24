using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Employees;
using Domain.LeaveRequests;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.LeaveRequests.GetMyLeaveRequests;

/// <summary>
/// Dashboard W1 handler: Returns the 5 most recent leave requests created by the current logged-in user.
/// Scope: strictly personal (Employee.UserId == currentUser.Id).
/// Read-only. No DB mutation.
/// </summary>
internal sealed class GetMyLeaveRequestsQueryHandler : IQueryHandler<GetMyLeaveRequestsQuery, List<MyLeaveRequestItem>>
{
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUserContext _userContext;

    public GetMyLeaveRequestsQueryHandler(
        ILeaveRequestRepository leaveRequestRepository,
        IUserRepository userRepository,
        IEmployeeRepository employeeRepository,
        IUserContext userContext)
    {
        _leaveRequestRepository = leaveRequestRepository;
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
        _userContext = userContext;
    }

    public async Task<Result<List<MyLeaveRequestItem>>> Handle(GetMyLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        string identityId = _userContext.IdentityId;

        // Resolve current user -> employee
        var user = await _userRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(u => u.IdentityId == new IdentityId(identityId), cancellationToken);

        if (user == null)
        {
            return Result.Success(new List<MyLeaveRequestItem>());
        }

        var employee = await _employeeRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(e => e.UserId == user.Id && e.IsActive, cancellationToken);

        if (employee == null)
        {
            return Result.Success(new List<MyLeaveRequestItem>());
        }

        // Query: my requests only, top 5, ordered by newest first
        var items = await _leaveRequestRepository.GetEntitiesAsQueryable()
            .Include(lr => lr.LeaveType)
            .Where(lr => lr.EmployeeId == employee.Id)
            .OrderByDescending(lr => lr.CreatedAt)
            .Take(5)
            .Select(lr => new MyLeaveRequestItem
            {
                Id = lr.Id.Value,
                LeaveTypeName = lr.LeaveType != null ? lr.LeaveType.Name : "Unknown",
                StartDate = lr.StartDate,
                EndDate = lr.EndDate,
                Duration = lr.Duration,
                Status = lr.Status.ToString(),
                CreatedAt = lr.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result.Success(items);
    }
}
