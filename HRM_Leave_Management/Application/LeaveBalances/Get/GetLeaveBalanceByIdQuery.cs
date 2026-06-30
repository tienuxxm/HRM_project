using Application.Abstractions.Messaging;

namespace Application.LeaveBalances.Get;

public sealed record GetLeaveBalanceByIdQuery(Guid Id) : IQuery<LeaveBalanceResponse>;
