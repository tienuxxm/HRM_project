using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.LeaveTypes;

namespace Application.LeaveTypes.Update;

internal sealed class UpdateLeaveTypeCommandHandler : ICommandHandler<UpdateLeaveTypeCommand, LeaveType>
{
    private readonly ILeaveTypeRepository _leaveTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateLeaveTypeCommandHandler(ILeaveTypeRepository leaveTypeRepository, IUnitOfWork unitOfWork)
    {
        _leaveTypeRepository = leaveTypeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LeaveType>> Handle(UpdateLeaveTypeCommand request,
        CancellationToken cancellationToken)
    {
        var leaveType = await _leaveTypeRepository.GetByIdAsync(
            new LeaveTypeId(request.Id), cancellationToken);

        if (leaveType is null)
            return Result.Failure<LeaveType>(LeaveTypeErrors.NotFound);

        var isDuplicateCode = await _leaveTypeRepository.IsExistedAsync(
            x => x.Code == request.Code && x.Id != new LeaveTypeId(request.Id));
        if (isDuplicateCode)
            return Result.Failure<LeaveType>(LeaveTypeErrors.LeaveTypeExisted);

        leaveType.Update(
            request.Name,
            request.Code,
            request.DefaultDays,
            request.Description);

        _leaveTypeRepository.Update(leaveType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return leaveType;
    }
}
