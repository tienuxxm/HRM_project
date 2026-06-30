using Application.Abstractions.Messaging;

namespace Application.LeaveRequests.Get;

public sealed record GetLeaveRequestsQuery(
    Guid? EmployeeId = null,
    Guid? LeaveTypeId = null,
    int? Status = null) : IQuery<List<LeaveRequestResponse>>;
