using Application.Abstractions.Messaging;
using Application.Response;
using Domain.Abstractions;
using Domain.LeaveBalances;

namespace Application.LeaveBalances.Update;

internal sealed class UpdateLeaveBalanceCommandHandler : ICommandHandler<UpdateLeaveBalanceCommand, BooleanResponse>
{
    private readonly ILeaveBalanceRepository _leaveBalanceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateLeaveBalanceCommandHandler(
        ILeaveBalanceRepository leaveBalanceRepository,
        IUnitOfWork unitOfWork)
    {
        _leaveBalanceRepository = leaveBalanceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BooleanResponse>> Handle(UpdateLeaveBalanceCommand request, CancellationToken cancellationToken)
    {
        var leaveBalanceId = new LeaveBalanceId(request.Id);
        var leaveBalance = await _leaveBalanceRepository.GetByIdAsync(leaveBalanceId, cancellationToken);

        if (leaveBalance == null || !leaveBalance.IsActive)
        {
            return Result.Failure<BooleanResponse>(LeaveBalanceErrors.NotFound);
        }

        leaveBalance.Update(request.AllocatedDays, request.UsedDays);

        _leaveBalanceRepository.Update(leaveBalance);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new BooleanResponse
        {
            Result = true,
            Message = "Leave balance updated successfully."
        });
    }
}
