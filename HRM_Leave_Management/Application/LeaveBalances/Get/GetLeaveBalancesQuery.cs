using Application.Abstractions.Messaging;
using Domain.Abstractions;

namespace Application.LeaveBalances.Get;

public sealed record GetLeaveBalancesQuery(
    Guid? EmployeeId = null,
    Guid? LeaveTypeId = null,
    int? Year = null,
    int Page = 1,
    int PageSize = 5) : IQuery<PagedList<LeaveBalanceResponse>>;
