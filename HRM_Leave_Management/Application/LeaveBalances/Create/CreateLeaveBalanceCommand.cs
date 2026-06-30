using Application.Abstractions.Messaging;
using Application.Response;

namespace Application.LeaveBalances.Create;

public sealed record CreateLeaveBalanceCommand(
    Guid EmployeeId,
    Guid LeaveTypeId,
    int Year,
    decimal AllocatedDays,
    decimal UsedDays = 0) : ICommand<BooleanResponse>;
