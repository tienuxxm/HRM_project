using Application.Abstractions.Messaging;
using Application.Response;
using Domain.Abstractions;
using Domain.LeaveBalances;

namespace Application.LeaveBalances.Delete;

internal sealed class DeleteLeaveBalanceCommandHandler : ICommandHandler<DeleteLeaveBalanceCommand, BooleanResponse>
{
    private readonly ILeaveBalanceRepository _leaveBalanceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteLeaveBalanceCommandHandler(
        ILeaveBalanceRepository leaveBalanceRepository,
        IUnitOfWork unitOfWork)
    {
        _leaveBalanceRepository = leaveBalanceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BooleanResponse>> Handle(DeleteLeaveBalanceCommand request, CancellationToken cancellationToken)
    {
        var leaveBalanceId = new LeaveBalanceId(request.Id);
        var leaveBalance = await _leaveBalanceRepository.GetByIdAsync(leaveBalanceId, cancellationToken);

        if (leaveBalance == null || !leaveBalance.IsActive)
        {
            return Result.Failure<BooleanResponse>(LeaveBalanceErrors.NotFound);
        }

        // Soft delete bằng cách đặt IsActive = false
        leaveBalance.SetActive(false);

        _leaveBalanceRepository.Update(leaveBalance);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new BooleanResponse
        {
            Result = true,
            Message = "Leave balance deleted successfully (soft delete)."
        });
    }
}
