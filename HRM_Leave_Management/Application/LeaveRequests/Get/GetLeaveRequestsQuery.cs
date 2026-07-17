using Application.Abstractions.Messaging;
using Domain.Abstractions;

namespace Application.LeaveRequests.Get;

public sealed record GetLeaveRequestsQuery(
    Guid? EmployeeId = null,
    Guid? LeaveTypeId = null,
    int? Status = null,
    int Page = 1,
    int PageSize = 5) : IQuery<PagedList<LeaveRequestResponse>>;
