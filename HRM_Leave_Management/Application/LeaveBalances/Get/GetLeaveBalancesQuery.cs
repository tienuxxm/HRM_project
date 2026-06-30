using Application.Abstractions.Messaging;

namespace Application.LeaveBalances.Get;

public sealed record GetLeaveBalancesQuery(
    Guid? EmployeeId = null,
    Guid? LeaveTypeId = null,
    int? Year = null) : IQuery<List<LeaveBalanceResponse>>;
