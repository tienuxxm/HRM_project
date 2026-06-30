using Application.Abstractions.Messaging;
using Application.Response;
using Domain.Abstractions;
using Domain.LeaveTypes;

namespace Application.LeaveTypes.Create;

internal sealed class CreateLeaveTypeCommandHandler : ICommandHandler<CreateLeaveTypeCommand, BooleanResponse>
{
    private readonly ILeaveTypeRepository _leaveTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLeaveTypeCommandHandler(ILeaveTypeRepository leaveTypeRepository, IUnitOfWork unitOfWork)
    {
        _leaveTypeRepository = leaveTypeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BooleanResponse>> Handle(CreateLeaveTypeCommand request,
        CancellationToken cancellationToken)
    {
        var isLeaveTypeExisted = await _leaveTypeRepository.IsExistedAsync(x => x.Code == request.Code);
        if (isLeaveTypeExisted)
            return Result.Failure<BooleanResponse>(LeaveTypeErrors.LeaveTypeExisted);

        var leaveType = LeaveType.Create(
            request.Name,
            request.Code,
            request.DefaultDays,
            request.Description);

        _leaveTypeRepository.Add(leaveType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new BooleanResponse
            { Result = true, Message = $"Leave type: {request.Name} has been created" });
    }
}
