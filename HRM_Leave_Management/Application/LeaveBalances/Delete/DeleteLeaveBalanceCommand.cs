using Application.Abstractions.Messaging;
using Application.Response;

namespace Application.LeaveBalances.Delete;

public sealed record DeleteLeaveBalanceCommand(Guid Id) : ICommand<BooleanResponse>;
