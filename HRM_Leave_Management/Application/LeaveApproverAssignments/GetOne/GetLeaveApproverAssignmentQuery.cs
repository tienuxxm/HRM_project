using Application.Abstractions.Messaging;
using Domain.LeaveApproverAssignments;

namespace Application.LeaveApproverAssignments.GetOne;

public sealed record GetLeaveApproverAssignmentQuery(Guid Id) : IQuery<LeaveApproverAssignment>;
