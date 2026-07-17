using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.LeaveTypes;

namespace Application.LeaveTypes.GetPaged;

public sealed record GetPagedLeaveTypesQuery(
    int Page = 1,
    int PageSize = 5) : IQuery<PagedList<LeaveType>>;
