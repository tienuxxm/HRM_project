using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.LeaveTypes;

namespace Application.LeaveTypes.GetPaged;

internal sealed class GetPagedLeaveTypesQueryHandler : IQueryHandler<GetPagedLeaveTypesQuery, PagedList<LeaveType>>
{
    private readonly ILeaveTypeRepository _leaveTypeRepository;

    public GetPagedLeaveTypesQueryHandler(ILeaveTypeRepository leaveTypeRepository)
    {
        _leaveTypeRepository = leaveTypeRepository;
    }

    public async Task<Result<PagedList<LeaveType>>> Handle(GetPagedLeaveTypesQuery request,
        CancellationToken cancellationToken)
    {
        var allLeaveTypes = await _leaveTypeRepository.GetAll(cancellationToken);
        var activeList = allLeaveTypes?.Where(x => x.IsActive).ToList() ?? new List<LeaveType>();

        // Clamp page/pageSize
        var pageSize = Math.Max(1, Math.Min(request.PageSize, 100));
        var totalCount = activeList.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var page = Math.Max(1, Math.Min(request.Page, Math.Max(1, totalPages)));

        var items = activeList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedList<LeaveType>(items, totalCount, page, pageSize);
    }
}
