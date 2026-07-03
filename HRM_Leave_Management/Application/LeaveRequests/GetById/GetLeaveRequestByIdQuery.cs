using Application.Abstractions.Messaging;
using Application.LeaveRequests.Get;
using Domain.Abstractions;

namespace Application.LeaveRequests.GetById;

public sealed record GetLeaveRequestByIdQuery(Guid Id) : IQuery<LeaveRequestResponse>;
