using Application.Abstractions.Messaging;
using Domain.LeaveTypes;

namespace Application.LeaveTypes.GetAll;

public sealed record GetAllLeaveTypesQuery : IQuery<List<LeaveType>>;
