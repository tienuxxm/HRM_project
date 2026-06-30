using Application.Abstractions.Messaging;
using Application.Response;

namespace Application.LeaveBalances.Update;

public sealed record UpdateLeaveBalanceCommand(
    Guid Id,
    decimal AllocatedDays,
    decimal UsedDays) : ICommand<BooleanResponse>;
