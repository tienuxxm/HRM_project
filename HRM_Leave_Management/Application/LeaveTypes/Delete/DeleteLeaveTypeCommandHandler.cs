using Application.Abstractions.Messaging;
using Application.Response;
using Domain.Abstractions;
using Domain.LeaveTypes;

namespace Application.LeaveTypes.Delete;

internal sealed class DeleteLeaveTypeCommandHandler : ICommandHandler<DeleteLeaveTypeCommand, BooleanResponse>
{
    private readonly ILeaveTypeRepository _leaveTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteLeaveTypeCommandHandler(ILeaveTypeRepository leaveTypeRepository, IUnitOfWork unitOfWork)
    {
        _leaveTypeRepository = leaveTypeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BooleanResponse>> Handle(DeleteLeaveTypeCommand request,
        CancellationToken cancellationToken)
    {
        var leaveType = await _leaveTypeRepository.GetByIdAsync(
            new LeaveTypeId(request.Id), cancellationToken);

        if (leaveType is null)
            return Result.Failure<BooleanResponse>(LeaveTypeErrors.NotFound);

        leaveType.SetActive(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new BooleanResponse
            { Result = true, Message = $"Leave type: {leaveType.Name} has been deactivated" });
    }
}
