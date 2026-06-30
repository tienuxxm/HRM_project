using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.LeaveTypes;

namespace Application.LeaveTypes.GetAll;

internal sealed class GetAllLeaveTypesQueryHandler : IQueryHandler<GetAllLeaveTypesQuery, List<LeaveType>>
{
    private readonly ILeaveTypeRepository _leaveTypeRepository;

    public GetAllLeaveTypesQueryHandler(ILeaveTypeRepository leaveTypeRepository)
    {
        _leaveTypeRepository = leaveTypeRepository;
    }

    public async Task<Result<List<LeaveType>>> Handle(GetAllLeaveTypesQuery request,
        CancellationToken cancellationToken)
    {
        var leaveTypes = await _leaveTypeRepository.GetAll(cancellationToken);
        return leaveTypes?.Where(x => x.IsActive).ToList() ?? new List<LeaveType>();
    }
}
